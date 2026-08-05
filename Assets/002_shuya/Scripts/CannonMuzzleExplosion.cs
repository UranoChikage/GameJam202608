using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 大砲の発射時に、発射炎・煙・火花のParticleをまとめて再生する。
/// このPrefabを大砲の発射口の子に置き、PlayExplosion()を呼び出して使用する。
/// </summary>
public sealed class CannonMuzzleExplosion : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField, Min(0.1f)] private float effectScale = 1f;

    [Header("Test")]
    [Tooltip("オンの場合、Play中にShiftキーを押すとエフェクトを再生する。")]
    [SerializeField] private bool testWithShiftKey;

    private ParticleSystem flashParticles;
    private ParticleSystem coreParticles;
    private ParticleSystem smokeParticles;
    private ParticleSystem sparkParticles;
    private ParticleSystem shockwaveParticles;
    private Light flashLight;
    private Coroutine lightCoroutine;

    private void Awake()
    {
        // Particle Systemは実行時に一度だけ組み立てる。
        flashParticles = CreateFlashParticles();
        coreParticles = CreateCoreParticles();
        smokeParticles = CreateSmokeParticles();
        sparkParticles = CreateSparkParticles();
        shockwaveParticles = CreateShockwaveParticles();
        flashLight = CreateFlashLight();
    }

    private void Update()
    {
        // テストが有効なオブジェクトだけ、左右どちらかのShiftキーで再生する。
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
    /// 大砲の爆発エフェクトを再生する公開関数。
    /// 大砲の発射処理からこの関数を呼び出す。
    /// </summary>
    public void PlayExplosion()
    {
        // 連射したときも、各Particleを発射口から追加で放出する。
        coreParticles.Emit(1);
        flashParticles.Emit(5);
        smokeParticles.Emit(9);
        sparkParticles.Emit(22);
        shockwaveParticles.Emit(1);

        if (lightCoroutine != null)
        {
            StopCoroutine(lightCoroutine);
        }
        lightCoroutine = StartCoroutine(FlashLight());
    }

    private ParticleSystem CreateCoreParticles()
    {
        ParticleSystem particles = CreateParticleObject("White Hot Core");
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 0.055f;
        main.startSpeed = 0.2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.85f, 1.15f);
        main.startColor = new Color(1f, 0.95f, 0.72f, 1f);

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = FadeGradient(Color.white, new Color(1f, 0.55f, 0.05f, 0f));
        return particles;
    }

    private ParticleSystem CreateFlashParticles()
    {
        ParticleSystem particles = CreateParticleObject("Muzzle Flash");
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.07f, 0.18f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.45f, 1.15f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.15f, 0.01f, 0.9f),
            new Color(1f, 0.85f, 0.15f, 1f));

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 17f;
        shape.radius = 0.05f;

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = FadeGradient(Color.white, new Color(1f, 0.1f, 0f, 0f));
        return particles;
    }

    private ParticleSystem CreateSmokeParticles()
    {
        ParticleSystem particles = CreateParticleObject("Muzzle Smoke");
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.85f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.18f, 0.18f, 0.18f, 0.65f),
            new Color(0.45f, 0.42f, 0.38f, 0.5f));

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.12f;

        ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1.8f));

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = FadeGradient(Color.white, new Color(0.3f, 0.3f, 0.3f, 0f));

        // ノイズで煙の軌道を崩し、一直線に見えないようにする。
        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 0.65f;
        noise.scrollSpeed = 0.25f;
        noise.damping = true;
        return particles;
    }

    private ParticleSystem CreateSparkParticles()
    {
        ParticleSystem particles = CreateParticleObject("Muzzle Sparks");
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.09f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.25f, 0.01f, 1f),
            new Color(1f, 0.95f, 0.25f, 1f));
        main.gravityModifier = 0.8f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 38f;
        shape.radius = 0.03f;

        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2.5f;
        renderer.velocityScale = 0.08f;

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = FadeGradient(Color.white, new Color(1f, 0.1f, 0f, 0f));
        return particles;
    }

    private ParticleSystem CreateShockwaveParticles()
    {
        ParticleSystem particles = CreateParticleObject("Pressure Wave");
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 0.18f;
        main.startSpeed = 0f;
        main.startSize = 0.25f;
        main.startColor = new Color(1f, 0.65f, 0.25f, 0.3f);

        ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f,
            AnimationCurve.EaseInOut(0f, 0.2f, 1f, 5f));

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = FadeGradient(new Color(1f, 1f, 1f, 0.3f), Color.clear);
        return particles;
    }

    private Light CreateFlashLight()
    {
        GameObject lightObject = new GameObject("Muzzle Light", typeof(Light));
        lightObject.transform.SetParent(transform, false);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.38f, 0.08f);
        light.range = 6f * effectScale;
        light.intensity = 0f;
        light.shadows = LightShadows.None;
        return light;
    }

    private IEnumerator FlashLight()
    {
        const float duration = 0.1f;
        float elapsed = 0f;
        flashLight.intensity = 7f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            flashLight.intensity = Mathf.Lerp(7f, 0f, elapsed / duration);
            yield return null;
        }

        flashLight.intensity = 0f;
        lightCoroutine = null;
    }

    private ParticleSystem CreateParticleObject(string objectName)
    {
        GameObject child = new GameObject(objectName, typeof(ParticleSystem));
        child.transform.SetParent(transform, false);
        child.transform.localScale = Vector3.one * effectScale;

        ParticleSystem particles = child.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 64;

        // 自動放出は使わず、PlayExplosion()のEmitで必要な数だけ出す。
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;

        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();
        return particles;
    }

    private static Material CreateParticleMaterial()
    {
        // URP用Particle Shaderを使い、見つからない場合はSprite Shaderへ切り替える。
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader)
        {
            name = "Runtime Muzzle Particle Material",
            hideFlags = HideFlags.HideAndDontSave
        };
        material.mainTexture = CreateSoftParticleTexture();
        return material;
    }

    private static Texture2D CreateSoftParticleTexture()
    {
        const int textureSize = 32;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            name = "Runtime Soft Particle Texture",
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color[] pixels = new Color[textureSize * textureSize];
        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.5f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Clamp01(1f - distance);
                alpha *= alpha;
                pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private static ParticleSystem.MinMaxGradient FadeGradient(Color start, Color end)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(start, 0f),
                new GradientColorKey(end, 1f)
            },
            new[]
            {
                new GradientAlphaKey(start.a, 0f),
                new GradientAlphaKey(end.a, 1f)
            });
        return new ParticleSystem.MinMaxGradient(gradient);
    }
}
