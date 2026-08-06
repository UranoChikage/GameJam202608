using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
    }
}
