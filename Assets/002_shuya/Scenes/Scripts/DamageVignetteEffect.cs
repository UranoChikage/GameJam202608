using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>


/// ダメージを受けたとき、画面の周囲を一瞬赤くする関数 PlayDamageEffect()


/// Global Volumeと同じGameObjectに付けて使用する。
/// </summary>
public sealed class DamageVignetteEffect : MonoBehaviour
{
    public static DamageVignetteEffect ActiveInstance { get; private set; }

    // ダメージ演出に使用するVolume。Inspectorから設定できる。
    [SerializeField] private Volume volume;

    // ダメージを受けた瞬間のVignetteの強さ。
    [SerializeField, Range(0f, 1f)] private float damageIntensity = 0.55f;

    // 赤いVignetteが通常の状態へ戻るまでの秒数。
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.4f;

    [Header("ヒール・ブースト")]
    [SerializeField] private Color healColor = new Color(0.1f, 1f, 0.25f, 1f);
    [SerializeField, Range(0f, 1f)] private float healIntensity = 0.24f;
    [SerializeField, Min(0.01f)] private float healDuration = 0.9f;
    [SerializeField, Range(0.01f, 1f)] private float healSmoothness = 0.18f;

    [SerializeField] private Color boostColor = new Color(1f, 0.85f, 0.05f, 1f);
    [SerializeField, Range(0f, 1f)] private float boostIntensity = 0.58f;
    [SerializeField, Min(0.01f)] private float boostDuration = 10f;

    [Header("黒い縁の鼓動")]
    // 1分間の心拍数。二連の脈動を1拍として扱う。
    [SerializeField, Min(1f)] private float pulseBpm = 72f;

    // 鼓動した瞬間に追加するVignetteの強さ。
    [SerializeField, Range(0f, 1f)] private float pulseStrength = 0.05f;

    // Volume Profileに入っているVignetteの設定を保存する変数。
    private Vignette vignette;

    // 現在実行しているフェード処理。連続ダメージ時に停止するため保存する。
    private Coroutine fadeCoroutine;

    // 演出後に戻すため、ゲーム開始時の色と強さを保存しておく。
    private Color defaultColor;
    private float defaultIntensity;
    private float defaultSmoothness;

    // ゲーム開始時に一度だけ呼ばれる初期化処理。
    private void Awake()
    {
        ActiveInstance = this;

        // InspectorでVolumeが設定されていなければ、同じGameObjectから探す。
        if (volume == null)
        {
            volume = GetComponent<Volume>();
        }

        // VolumeやVignetteが見つからない場合は、エラーを表示して処理を止める。
        if (volume == null || volume.profile == null || !volume.profile.TryGet(out vignette))
        {
            Debug.LogError("DamageVignetteEffect: Vignetteを含むVolumeが必要です。", this);
            enabled = false;
            return;
        }

        // ダメージ演出が終わったときに戻す、通常時の設定を記録する。
        defaultColor = vignette.color.value;
        defaultIntensity = vignette.intensity.value;
        defaultSmoothness = vignette.smoothness.value;
    }

    // 毎フレーム呼ばれる処理。
    private void Update()
    {
        // ダメージ演出中でなければ、黒い縁を二連の心拍リズムで動かす。
        if (fadeCoroutine == null)
        {
            vignette.color.value = defaultColor;
            vignette.intensity.value = GetPulseIntensity();
        }

        // Hキーで緑のヒール演出、Bキーで黄色のブースト演出を確認できる。
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            PlayHealEffect();
        }

        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            PlayBoostEffect();
        }
    }

    /// <summary>
    /// 赤いダメージ演出を再生する。
    /// publicなので、プレイヤーのHP処理など別のスクリプトからも呼び出せる。
    /// </summary>
    public void PlayDamageEffect()
    {
        PlayEffect(Color.red, damageIntensity, fadeDuration);
    }

    /// <summary>現在のシーンに登録されたポストプロセスへダメージを通知する。</summary>
    public static bool TryPlayDamageEffect()
    {
        if (ActiveInstance == null)
        {
            ActiveInstance = FindFirstObjectByType<DamageVignetteEffect>();
        }

        if (ActiveInstance == null || !ActiveInstance.isActiveAndEnabled)
        {
            Debug.LogError("ダメージ用のDamageVignetteEffectが有効なシーン内に見つかりません。");
            return false;
        }

        ActiveInstance.PlayDamageEffect();
        return true;
    }

    /// <summary>緑のヒール演出を再生する。Hキーからも呼ばれる。</summary>
    public void PlayHealEffect()
    {
        RestartEffect(FadeHealAtScreenEdge());
    }

    /// <summary>現在のシーンに登録されたポストプロセスへヒールを通知する。</summary>
    public static bool TryPlayHealEffect()
    {
        if (ActiveInstance == null)
        {
            ActiveInstance = FindFirstObjectByType<DamageVignetteEffect>();
        }

        if (ActiveInstance == null || !ActiveInstance.isActiveAndEnabled)
        {
            Debug.LogError("ヒール用のDamageVignetteEffectが有効なシーン内に見つかりません。");
            return false;
        }

        ActiveInstance.PlayHealEffect();
        return true;
    }

    /// <summary>黄色のブースト演出を再生する。Bキーからも呼ばれる。</summary>
    public void PlayBoostEffect()
    {
        PlayBoostEffect(boostDuration);
    }

    /// <summary>指定された効果時間だけ黄色のブースト演出を表示する。</summary>
    public void PlayBoostEffect(float duration)
    {
        RestartEffect(HoldBoostVignette(Mathf.Max(0.01f, duration)));
    }

    /// <summary>現在のシーンに登録されたポストプロセスへブーストを通知する。</summary>
    public static bool TryPlayBoostEffect(float duration)
    {
        // シーン読み込み順の都合で未登録なら、一度だけシーン内から探して補完する。
        if (ActiveInstance == null)
        {
            ActiveInstance = FindFirstObjectByType<DamageVignetteEffect>();
        }

        if (ActiveInstance == null || !ActiveInstance.isActiveAndEnabled)
        {
            Debug.LogError("ブースト用のDamageVignetteEffectが有効なシーン内に見つかりません。");
            return false;
        }

        ActiveInstance.PlayBoostEffect(duration);
        return true;
    }

    private void PlayEffect(Color effectColor, float effectIntensity, float duration)
    {
        RestartEffect(FadeVignette(effectColor, effectIntensity, duration));
    }

    private void RestartEffect(IEnumerator effect)
    {
        // すでに演出中なら止め、連続入力でも鮮明に光らせ直す。
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            vignette.smoothness.value = defaultSmoothness;
        }

        fadeCoroutine = StartCoroutine(effect);
    }

    /// <summary>画面の外周だけを、じんわり緑色にして戻すヒール演出。</summary>
    private IEnumerator FadeHealAtScreenEdge()
    {
        vignette.color.Override(healColor);
        vignette.smoothness.Override(healSmoothness);

        float elapsed = 0f;
        while (elapsed < healDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / healDuration);

            // 前半でゆっくり現れ、少し保ったあと後半でゆっくり消える。
            float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.35f));
            float fadeOut = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.55f) / 0.45f));
            float visibility = Mathf.Min(fadeIn, fadeOut);

            vignette.color.value = Color.Lerp(defaultColor, healColor, visibility);
            vignette.intensity.value = Mathf.Lerp(GetPulseIntensity(), healIntensity, visibility);
            yield return null;
        }

        vignette.color.value = defaultColor;
        vignette.intensity.value = GetPulseIntensity();
        vignette.smoothness.value = defaultSmoothness;
        fadeCoroutine = null;
    }

    /// <summary>黄色のブースト表示を維持し、終了直前に滑らかに消す。</summary>
    private IEnumerator HoldBoostVignette(float duration)
    {
        vignette.color.Override(boostColor);

        float elapsed = 0f;
        const float fadeInDuration = 0.2f;
        const float fadeOutDuration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float fadeIn = Mathf.Clamp01(elapsed / fadeInDuration);
            float fadeOut = Mathf.Clamp01((duration - elapsed) / fadeOutDuration);
            float visibility = Mathf.SmoothStep(0f, 1f, Mathf.Min(fadeIn, fadeOut));

            vignette.color.value = Color.Lerp(defaultColor, boostColor, visibility);
            vignette.intensity.value = Mathf.Lerp(GetPulseIntensity(), boostIntensity, visibility);
            yield return null;
        }

        vignette.color.value = defaultColor;
        vignette.intensity.value = GetPulseIntensity();
        fadeCoroutine = null;
    }

    /// <summary>
    /// Vignetteを赤くした後、fadeDuration秒かけて通常の状態へ戻す。
    /// </summary>
    private IEnumerator FadeVignette(Color effectColor, float effectIntensity, float duration)
    {
        vignette.color.Override(effectColor);
        vignette.intensity.Override(effectIntensity);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Time.unscaledDeltaTimeを使うため、ゲームを一時停止していても演出が進む。
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // 最初は少し粘り、後半で滑らかに消える。
            float fadeT = t * t * (3f - 2f * t);

            vignette.color.value = Color.Lerp(effectColor, defaultColor, fadeT);
            vignette.intensity.value = Mathf.Lerp(effectIntensity, GetPulseIntensity(), fadeT);

            // 次のフレームまで待つ。
            yield return null;
        }

        // 最後に値を正確に通常時へ戻す。
        vignette.color.value = defaultColor;
        vignette.intensity.value = GetPulseIntensity();
        fadeCoroutine = null;
    }

    // 「ドク・ドク……」という二連拍の波形から、現在の強さを求める。
    private float GetPulseIntensity()
    {
        float phase = Mathf.Repeat(Time.unscaledTime * pulseBpm / 60f, 1f);
        float firstBeat = GetBeat(phase, 0.08f, 0.08f);
        float secondBeat = GetBeat(phase, 0.26f, 0.07f) * 0.65f;
        float heartbeat = Mathf.Max(firstBeat, secondBeat);

        return Mathf.Clamp01(defaultIntensity + heartbeat * pulseStrength);
    }

    // 周期の継ぎ目でも滑らかになる、1つ分の柔らかい脈動。
    private static float GetBeat(float phase, float center, float halfWidth)
    {
        float distance = Mathf.Abs(Mathf.Repeat(phase - center + 0.5f, 1f) - 0.5f);
        float beat = 1f - Mathf.Clamp01(distance / halfWidth);
        return beat * beat * (3f - 2f * beat);
    }

    // GameObjectやスクリプトが無効になったときに呼ばれる。
    private void OnDisable()
    {
        if (vignette == null)
        {
            return;
        }

        // 演出途中で無効になっても、赤い画面が残らないように元へ戻す。
        vignette.color.value = defaultColor;
        vignette.intensity.value = defaultIntensity;
        vignette.smoothness.value = defaultSmoothness;
    }

    private void OnDestroy()
    {
        if (ActiveInstance == this)
        {
            ActiveInstance = null;
        }
    }
}
