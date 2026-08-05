using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// JMO WarFXの既存Prefabだけを重ね、ひとつの大きな爆発として再生する。
/// 火球、破片、放射状の煙、残留煙を時間差で組み合わせている。
/// </summary>
public sealed class JmoLayeredExplosion : MonoBehaviour
{
    [Header("JMO WarFX Prefabs")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject starSmokePrefab;
    [SerializeField] private GameObject lingeringSmokePrefab;

    [Header("Composition")]
    [SerializeField, Min(0.05f)] private float effectScale = 0.55f;
    [SerializeField, Min(0f)] private float smokeDelay = 0.08f;

    [Header("Impact")]
    [SerializeField] private bool useCameraShake = true;
    [SerializeField, Range(0f, 0.5f)] private float shakeStrength = 0.09f;
    [SerializeField, Min(0.01f)] private float shakeDuration = 0.18f;

    [Header("Test")]
    [SerializeField] private bool testWithShiftKey;

    private Coroutine playCoroutine;
    private Light explosionLight;

    private void Awake()
    {
        // 爆発の瞬間だけ周囲を照らすPoint Lightを用意する。
        GameObject lightObject = new GameObject("JMO Explosion Flash Light", typeof(Light));
        lightObject.transform.SetParent(transform, false);
        explosionLight = lightObject.GetComponent<Light>();
        explosionLight.type = LightType.Point;
        explosionLight.color = new Color(1f, 0.32f, 0.04f);
        explosionLight.range = 12f * effectScale;
        explosionLight.intensity = 0f;
        explosionLight.shadows = LightShadows.None;
    }

    private void Update()
    {
        if (!testWithShiftKey || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
            Keyboard.current.rightShiftKey.wasPressedThisFrame)
        {
            PlayExplosion();
        }
    }

    /// <summary>
    /// JMO Assetsだけで構成した複合爆発を再生する。
    /// ゲームの爆発処理からいつでも呼び出せる。
    /// </summary>
    public void PlayExplosion()
    {
        // 最初の爆発はこの火球1回だけにする。
        SpawnEffect(fireballPrefab, 1.15f, Vector3.zero);

        // 強い光と画面振動で、爆発の重さを補強する。
        StartCoroutine(FlashExplosionLight());
        if (useCameraShake && Camera.main != null)
        {
            StartCoroutine(ShakeCamera(Camera.main.transform));
        }

        // 煙の時間差処理は、連続再生された場合もそれぞれ独立して残す。
        playCoroutine = StartCoroutine(SpawnSmokeLayers());
    }

    private IEnumerator SpawnSmokeLayers()
    {
        // 爆発を追加せず、少し遅れて煙だけを展開する。
        yield return new WaitForSecondsRealtime(smokeDelay);

        // 星形に広がる煙で爆風の方向性を出す。
        SpawnEffect(starSmokePrefab, 1.25f, new Vector3(0f, 0.08f, 0f));

        yield return new WaitForSecondsRealtime(0.05f);

        // 最後に大きい煙を残し、爆発後の余韻を作る。
        SpawnEffect(lingeringSmokePrefab, 1f, new Vector3(0f, 0.25f, 0f));

        // 上方にもう一層の煙を置き、参考画像のような縦長の黒煙を残す。
        yield return new WaitForSecondsRealtime(0.14f);
        SpawnEffect(lingeringSmokePrefab, 0.72f, new Vector3(0.05f, 0.9f, 0.02f));
        playCoroutine = null;
    }

    private IEnumerator FlashExplosionLight()
    {
        const float flashDuration = 0.16f;
        float elapsed = 0f;
        explosionLight.intensity = 14f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            explosionLight.intensity = Mathf.Lerp(14f, 0f, t * t);
            yield return null;
        }

        explosionLight.intensity = 0f;
    }

    private IEnumerator ShakeCamera(Transform cameraTransform)
    {
        Vector3 originalLocalPosition = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float remaining = 1f - Mathf.Clamp01(elapsed / shakeDuration);
            Vector2 offset = Random.insideUnitCircle * shakeStrength * remaining;
            cameraTransform.localPosition = originalLocalPosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }

        cameraTransform.localPosition = originalLocalPosition;
    }

    private void SpawnEffect(GameObject prefab, float layerScale, Vector3 localOffset)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject effect = Instantiate(prefab, transform.position, transform.rotation);
        effect.name = $"JMO Layer - {prefab.name}";
        effect.transform.position += transform.TransformVector(localOffset * effectScale);
        effect.transform.localScale = Vector3.one * effectScale * layerScale;

        // 毎回わずかに向きを変え、同じ爆発が繰り返されて見えるのを防ぐ。
        effect.transform.Rotate(Vector3.forward, Random.Range(0f, 360f), Space.Self);

        // Asset側の自動破棄に加え、安全のため一定時間後にも削除する。
        Destroy(effect, 8f);
    }
}
