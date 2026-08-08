using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class ChatNovelController : MonoBehaviour
{
    private static readonly Dictionary<TMP_Text, ChatNovelController> ControllersByText = new();

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TMP_Text messageText;
    [SerializeField, TextArea(2, 5)] private string introduction =
        "ネットの掲示板で噂になった「ドリームjoyの秘密の宝石」についてこんなスレが建てられた。";
    [SerializeField, Min(0.01f)] private float fadeDuration = 1.2f;
    [SerializeField, Min(0f)] private float introductionDuration = 2.5f;
    [SerializeField, Min(1f)] private float introductionFontSize = 72f;
    [SerializeField, Min(1f)] private float chatFontSize = 48f;
    [SerializeField] private string nextSceneName = "Stage0";
    [SerializeField, TextArea(2, 5)] private string[] messages =
    {
        "スレ民１「おい知っているか、ドリームjoyの工場にはでかい宝石があるんや！！」",
        "スレ民２「マジで( ﾟДﾟ)」",
        "スレ民３「嘘乙ｗｗｗ」",
        "スレ民４「嘘だろ」",
        "スレ民１「嘘じゃない」",
        "スレ民１「本当や！！ワイの友達そこで働いていて、でかい宝石見たって言ってた。」",
        "スレ民２「なんだ、イッチが見たわけじゃないのかよ(*_*;」",
        "スレ民３「やっぱり嘘じゃねぇか」",
        "主人公「それ本当？」",
        "スレ民１「本当や！！ワイの事信じてくれるのか？」",
        "主人公「まあな、真夜中に実際にその工場に入って宝石があるか見てくるは」",
        "スレ民１「サンガツ」",
        "スレ民２「期待して待ってる(*^^)v」",
        "スレ民３「普通に不法侵入で草」",
        "スレ民４「捕まんなよｗｗｗ」"
    };

    private int nextMessageIndex;
    private bool isUpdatingScroll;
    private bool canAdvance;
    private Image background;
    private bool isLoadingScene;

    private void Awake()
    {
        ResolveReferences();

        if (messageText == null)
        {
            enabled = false;
            return;
        }

        if (ControllersByText.TryGetValue(messageText, out ChatNovelController controller) && controller != null)
        {
            enabled = false;
            return;
        }

        ControllersByText[messageText] = this;
        ConfigureLayout();
        StartCoroutine(PlayIntroduction());
    }

    private void OnDestroy()
    {
        if (messageText != null && ControllersByText.TryGetValue(messageText, out ChatNovelController controller) && controller == this)
        {
            ControllersByText.Remove(messageText);
        }
    }

    private void Update()
    {
        if (canAdvance && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShowNextMessage();
        }
    }

    private void ResolveReferences()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        if (messageText == null && scrollRect != null && scrollRect.content != null)
        {
            messageText = scrollRect.content.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void ConfigureLayout()
    {
        if (scrollRect == null || scrollRect.content == null || messageText == null)
        {
            return;
        }

        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        background = scrollRect.GetComponent<Image>();
        if (background != null)
        {
            background.color = Color.black;
        }

        if (scrollRect.viewport != null)
        {
            scrollRect.viewport.anchorMin = Vector2.zero;
            scrollRect.viewport.anchorMax = Vector2.one;
            scrollRect.viewport.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.viewport.anchoredPosition = Vector2.zero;
            scrollRect.viewport.sizeDelta = Vector2.zero;
        }

        RectTransform content = scrollRect.content;
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.padding = new RectOffset(72, 72, 56, 56);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform textRect = messageText.rectTransform;
        textRect.localScale = Vector3.one;
        messageText.margin = Vector4.zero;
        messageText.fontSize = chatFontSize;
        messageText.alignment = TextAlignmentOptions.TopLeft;
        messageText.textWrappingMode = TextWrappingModes.Normal;
        messageText.raycastTarget = false;
    }

    private IEnumerator PlayIntroduction()
    {
        canAdvance = false;
        messageText.text = string.Empty;

        TMP_Text introductionText = Instantiate(messageText, scrollRect.transform);
        introductionText.name = "Introduction Text";
        introductionText.text = introduction;
        introductionText.fontSize = introductionFontSize;
        introductionText.alignment = TextAlignmentOptions.Center;
        introductionText.raycastTarget = false;

        RectTransform introductionRect = introductionText.rectTransform;
        introductionRect.anchorMin = Vector2.zero;
        introductionRect.anchorMax = Vector2.one;
        introductionRect.pivot = new Vector2(0.5f, 0.5f);
        introductionRect.anchoredPosition = Vector2.zero;
        introductionRect.sizeDelta = Vector2.zero;
        introductionRect.SetAsLastSibling();

        CanvasGroup introductionCanvasGroup = introductionText.GetComponent<CanvasGroup>();
        if (introductionCanvasGroup == null)
        {
            introductionCanvasGroup = introductionText.gameObject.AddComponent<CanvasGroup>();
        }

        introductionCanvasGroup.alpha = 0f;

        yield return FadeText(introductionCanvasGroup, 0f, 1f);
        yield return new WaitForSecondsRealtime(introductionDuration);
        yield return FadeOutIntroduction(introductionCanvasGroup);

        Destroy(introductionText.gameObject);
        ShowNextMessage();
        canAdvance = true;
    }

    private IEnumerator FadeText(CanvasGroup target, float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            target.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        target.alpha = to;
    }

    private IEnumerator FadeOutIntroduction(CanvasGroup target)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            target.alpha = alpha;
            SetBackgroundAlpha(alpha);
            yield return null;
        }

        target.alpha = 0f;
        SetBackgroundAlpha(0f);
    }

    private void SetBackgroundAlpha(float alpha)
    {
        if (background == null)
        {
            return;
        }

        Color color = background.color;
        color.a = alpha;
        background.color = color;
    }

    private void ShowNextMessage()
    {
        if (messageText == null || isLoadingScene)
        {
            return;
        }

        if (messages == null || nextMessageIndex >= messages.Length)
        {
            LoadNextScene();
            return;
        }

        string message = messages[nextMessageIndex];
        nextMessageIndex++;

        if (!string.IsNullOrWhiteSpace(message))
        {
            if (messageText.text.Length > 0)
            {
                messageText.text += "\n\n";
            }

            messageText.text += message;
        }

        if (!isUpdatingScroll)
        {
            StartCoroutine(ScrollToLatestMessage());
        }
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError("移動先のシーン名が設定されていません。", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError($"シーン '{nextSceneName}' がBuild Settingsに登録されていません。", this);
            return;
        }

        isLoadingScene = true;
        canAdvance = false;
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator ScrollToLatestMessage()
    {
        isUpdatingScroll = true;
        yield return null;
        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        isUpdatingScroll = false;
    }
}
