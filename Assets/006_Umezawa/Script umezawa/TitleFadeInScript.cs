using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public sealed class TitleFadeInScript : MonoBehaviour
{
    [Header("フェードイン設定")]
    [SerializeField, Min(0f)] private float fadeInTime = 1.5f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneEvent()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartFirstFade()
    {
        StartAllFadeObjects();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartAllFadeObjects();
    }

    private static void StartAllFadeObjects()
    {
        TitleFadeInScript[] fadeObjects = FindObjectsByType<TitleFadeInScript>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (TitleFadeInScript fadeObject in fadeObjects)
        {
            fadeObject.PlayFadeIn();
        }
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        PlayFadeIn();
    }

    private void PlayFadeIn()
    {
        if (fadeCoroutine != null)
        {
            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[TitleFadeInScript] 親GameObjectが非アクティブのため、フェードを開始できません。", this);
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        fadeCoroutine = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        // 最初のフレームでは完全な黒画面を確実に表示する。
        yield return null;

        float elapsedTime = 0f;

        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = fadeInTime <= 0f ? 1f : elapsedTime / fadeInTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(progress);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        fadeCoroutine = null;
        gameObject.SetActive(false);
    }
}
