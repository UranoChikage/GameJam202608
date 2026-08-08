using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 使い方：
///   - シーン内に1つだけ配置し、他のスクリプトから GameManager.Instance で参照する。
///   - Goal などから LoadScene(シーン名) を呼び出すとシーン遷移する。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("シーン遷移演出")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("UI - オプションメニュー")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button openOptionsButton;
    [SerializeField] private Button closeOptionsButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private string titleSceneName = "Title";

    [Header("UI - オプションボタンを表示するシーン")]
    [Tooltip("ここに含まれるシーンでのみOptionボタンを表示する（Novelパートのシーン名を入れる）")]
    [SerializeField] private string[] scenesWithOptionButton;

    [Header("アクションパートのシーン")]
    [Tooltip("ここに含まれるシーンをアクションパートとして扱う（オプション開閉でマウス表示を切り替える対象）")]
    [SerializeField] private string[] actionPartScenes;

    private bool isLoading;

    private bool IsActionPartScene(string sceneName)
    {
        return actionPartScenes != null &&
               System.Array.IndexOf(actionPartScenes, sceneName) >= 0;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (openOptionsButton != null)
            openOptionsButton.onClick.AddListener(OpenOptions);

        if (closeOptionsButton != null)
            closeOptionsButton.onClick.AddListener(CloseOptions);

        if (titleButton != null)
            titleButton.onClick.AddListener(GoToTitle);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryStage);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        UpdateOptionButtonVisibility(SceneManager.GetActiveScene().name);
    }

    private void UpdateOptionButtonVisibility(string sceneName)
    {
        if (openOptionsButton == null) return;

        bool show =
            scenesWithOptionButton != null &&
            System.Array.IndexOf(scenesWithOptionButton, sceneName) >= 0;

        openOptionsButton.gameObject.SetActive(show);
    }

    /// <summary>オプションボタンから呼ぶ。オプションメニューを開く。</summary>
    public void OpenOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (IsActionPartScene(SceneManager.GetActiveScene().name))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    /// <summary>オプションメニューを閉じる。</summary>
    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (IsActionPartScene(SceneManager.GetActiveScene().name))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>オプションメニューの「タイトルへ戻る」ボタンから呼ぶ。</summary>
    public void GoToTitle()
    {
        LoadScene(titleSceneName);
    }

    /// <summary>オプションメニューの「やり直し」ボタンから呼ぶ。現在のシーンを再読み込みする。</summary>
    public void RetryStage()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading || string.IsNullOrEmpty(sceneName)) return;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[GameManager] シーン '{sceneName}' が Build Settings に見つかりません。", this);
            return;
        }

        isLoading = true;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return FadeOut();

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>画面を暗転させる。死亡演出やシーン遷移演出から呼ぶ。</summary>
    public Coroutine FadeOut()
    {
        return StartCoroutine(FadeCanvas(0f, 1f));
    }

    /// <summary>暗転から復帰する。</summary>
    public Coroutine FadeIn()
    {
        return StartCoroutine(FadeCanvas(1f, 0f));
    }

    private IEnumerator FadeCanvas(float from, float to)
    {
        if (fadeCanvas == null)
            yield break;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = to;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoading = false;
        FadeIn();

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        UpdateOptionButtonVisibility(scene.name);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel != null)
            {
                if (optionsPanel.activeSelf)
                    CloseOptions();
                else
                    OpenOptions();
            }
        }
    }
}
