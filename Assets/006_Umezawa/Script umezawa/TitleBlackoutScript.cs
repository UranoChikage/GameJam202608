using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class TitleBlackoutScript : MonoBehaviour
{
    [Header("暗転の発生間隔（秒）")]
    [SerializeField, Min(0.1f)] private float minimumInterval = 8f;
    [SerializeField, Min(0.1f)] private float maximumInterval = 20f;

    [Header("暗転時間（秒）")]
    [SerializeField, Min(0.01f)] private float minimumDuration = 0.05f;
    [SerializeField, Min(0.01f)] private float maximumDuration = 0.2f;

    [Header("暗さ")]
    [SerializeField, Range(0f, 1f)] private float blackoutAlpha = 1f;

    private GameObject overlayObject;
    private CanvasGroup overlayCanvasGroup;

    private void Awake()
    {
        CreateBlackoutOverlay();
    }

    private IEnumerator Start()
    {
        while (true)
        {
            float interval = GetRandomRange(minimumInterval, maximumInterval, 0.1f);
            yield return new WaitForSecondsRealtime(interval);

            overlayCanvasGroup.alpha = blackoutAlpha;

            float duration = GetRandomRange(minimumDuration, maximumDuration, 0.01f);
            yield return new WaitForSecondsRealtime(duration);

            overlayCanvasGroup.alpha = 0f;
        }
    }

    private void OnDisable()
    {
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 0f;
        }
    }

    private void OnDestroy()
    {
        if (overlayObject != null)
        {
            Destroy(overlayObject);
        }
    }

    private void CreateBlackoutOverlay()
    {
        overlayObject = new GameObject("Title Blackout Overlay");

        Canvas canvas = overlayObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        overlayObject.AddComponent<CanvasScaler>();
        overlayCanvasGroup = overlayObject.AddComponent<CanvasGroup>();
        overlayCanvasGroup.alpha = 0f;
        overlayCanvasGroup.interactable = false;
        overlayCanvasGroup.blocksRaycasts = false;

        GameObject imageObject = new GameObject("Black Image");
        imageObject.transform.SetParent(overlayObject.transform, false);

        RectTransform imageTransform = imageObject.AddComponent<RectTransform>();
        imageTransform.anchorMin = Vector2.zero;
        imageTransform.anchorMax = Vector2.one;
        imageTransform.offsetMin = Vector2.zero;
        imageTransform.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;
    }

    private static float GetRandomRange(float firstValue, float secondValue, float minimumValue)
    {
        float minimum = Mathf.Max(minimumValue, Mathf.Min(firstValue, secondValue));
        float maximum = Mathf.Max(minimum, Mathf.Max(firstValue, secondValue));
        return Random.Range(minimum, maximum);
    }
}
