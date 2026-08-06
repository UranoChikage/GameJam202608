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
    // ダメージ演出に使用するVolume。Inspectorから設定できる。
    [SerializeField] private Volume volume;

    // ダメージを受けた瞬間のVignetteの強さ。
    [SerializeField, Range(0f, 1f)] private float damageIntensity = 0.55f;

    // 赤いVignetteが通常の状態へ戻るまでの秒数。
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.4f;

    // Volume Profileに入っているVignetteの設定を保存する変数。
    private Vignette vignette;

    // 現在実行しているフェード処理。連続ダメージ時に停止するため保存する。
    private Coroutine fadeCoroutine;

    // 演出後に戻すため、ゲーム開始時の色と強さを保存しておく。
    private Color defaultColor;
    private float defaultIntensity;

    // ゲーム開始時に一度だけ呼ばれる初期化処理。
    private void Awake()
    {
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
    }

    // 毎フレーム呼ばれる処理。
    private void Update()
    {
        // 動作確認用。スペースキーを押すとダメージ演出を再生する。
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayDamageEffect();
        }
    }

    /// <summary>
    /// 赤いダメージ演出を再生する。
    /// publicなので、プレイヤーのHP処理など別のスクリプトからも呼び出せる。
    /// </summary>
    public void PlayDamageEffect()
    {
        // すでに演出中なら一度止め、連続ダメージでも最初から光らせ直す。
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // 時間をかけて元へ戻すコルーチンを開始する。
        fadeCoroutine = StartCoroutine(FadeDamageVignette());
    }

    /// <summary>
    /// Vignetteを赤くした後、fadeDuration秒かけて通常の状態へ戻す。
    /// </summary>
    private IEnumerator FadeDamageVignette()
    {
        // ダメージを受けた瞬間は、Vignetteを赤くして強くする。
        vignette.color.Override(Color.red);
        vignette.intensity.Override(damageIntensity);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            // Time.unscaledDeltaTimeを使うため、ゲームを一時停止していても演出が進む。
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // Lerpで赤色と強さを少しずつ通常時の値へ近づける。
            vignette.color.value = Color.Lerp(Color.red, defaultColor, t);
            vignette.intensity.value = Mathf.Lerp(damageIntensity, defaultIntensity, t);

            // 次のフレームまで待つ。
            yield return null;
        }

        // 最後に値を正確に通常時へ戻す。
        vignette.color.value = defaultColor;
        vignette.intensity.value = defaultIntensity;
        fadeCoroutine = null;
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
    }
}
