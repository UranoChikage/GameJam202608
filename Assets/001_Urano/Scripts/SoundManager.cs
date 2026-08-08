using System.Collections;
using UnityEngine;

/// <summary>
/// 使い方：
///   - シーン内に1つだけ配置し、他のスクリプトから SoundManager.Instance で参照する。
///   - BGMは PlayBgm(clip) / StopBgm() で切り替える（自動でクロスフェードする）。
///   - SEは PlaySfx(clip, position) で好きな場所から1回鳴らす（専用AudioSource不要）。
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.6f;
    [SerializeField] private float bgmFadeDuration = 1f;

    [Header("SE")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("足音")]
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.5f;

    private Coroutine bgmRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0f;
    }

    public void PlayBgm(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;

        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);

        bgmRoutine = StartCoroutine(CrossfadeBgm(clip));
    }

    public void StopBgm()
    {
        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);

        bgmRoutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator CrossfadeBgm(AudioClip clip)
    {
        yield return FadeVolume(bgmSource.volume, 0f);

        bgmSource.clip = clip;
        bgmSource.Play();

        yield return FadeVolume(0f, bgmVolume);
    }

    private IEnumerator FadeOutAndStop()
    {
        yield return FadeVolume(bgmSource.volume, 0f);
        bgmSource.Stop();
    }

    private IEnumerator FadeVolume(float from, float to)
    {
        float timer = 0f;

        while (timer < bgmFadeDuration)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(from, to, timer / bgmFadeDuration);
            yield return null;
        }

        bgmSource.volume = to;
    }

    /// <summary>指定位置でSEを1回再生する。専用AudioSourceを持たないオブジェクトから呼ぶ想定。</summary>
    public void PlaySfx(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * volumeScale);
    }

    /// <summary>足音を再生する。SEとは別の音量枠で管理する。</summary>
    public void PlayFootstep(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, footstepVolume * volumeScale);
    }
}
