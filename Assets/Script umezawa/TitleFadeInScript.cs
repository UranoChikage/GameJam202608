using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TitleFadeInScript : MonoBehaviour
{
    [Header("フェードイン設定")]
    [SerializeField, Min(0f)] private float delay = 0f;
    [SerializeField, Min(0f)] private float duration = 1.5f;

    private CanvasGroup canvasGroup;
    private bool fadeStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartInactiveFadeObjects()
    {
        TitleFadeInScript[] fadeObjects = FindObjectsByType<TitleFadeInScript>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (TitleFadeInScript fadeObject in fadeObjects)
        {
            fadeObject.BeginFade();
        }
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
    }

    private void Start()
    {
        BeginFade();
    }

    private void BeginFade()
    {
        if (fadeStarted)
        {
            return;
        }

        fadeStarted = true;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        if (duration > 0f)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / duration);
                yield return null;
            }
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
