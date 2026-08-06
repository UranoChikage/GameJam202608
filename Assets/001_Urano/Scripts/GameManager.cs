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
        SceneManager.LoadScene(sceneName);
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
    }
}
