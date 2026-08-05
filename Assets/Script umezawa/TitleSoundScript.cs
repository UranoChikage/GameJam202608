using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TitleSoundScript : MonoBehaviour
{
    [Header("タイトル画面の効果音")]
    [SerializeField] private AudioClip titleSound;

    [Header("タイトル画面のBGM")]
    [SerializeField] private AudioClip bgm;
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField, Min(0f)] private float bgmFadeInDuration = 1.5f;
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("ランダムノイズ")]
    [SerializeField] private AudioClip[] noiseSounds;
    [SerializeField, Min(0.1f)] private float minimumNoiseInterval = 5f;
    [SerializeField, Min(0.1f)] private float maximumNoiseInterval = 15f;
    [SerializeField, Min(0.1f)] private float minimumNoiseDuration = 0.5f;
    [SerializeField, Min(0.1f)] private float maximumNoiseDuration = 2f;
    [SerializeField, Range(0f, 1f)] private float noiseVolume = 1f;
    [SerializeField] private AudioSource noiseAudioSource;

    private AudioSource soundEffectAudioSource;

    private void Awake()
    {
        soundEffectAudioSource = GetComponent<AudioSource>();
        soundEffectAudioSource.playOnAwake = false;

        if (bgmAudioSource == null || bgmAudioSource == soundEffectAudioSource)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }

        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.loop = true;

        if (noiseAudioSource == null ||
            noiseAudioSource == soundEffectAudioSource ||
            noiseAudioSource == bgmAudioSource)
        {
            noiseAudioSource = gameObject.AddComponent<AudioSource>();
        }

        noiseAudioSource.playOnAwake = false;
        noiseAudioSource.loop = true;
    }

    private void Start()
    {
        PlayTitleSound();
        PlayBgm();

        if (noiseSounds != null && noiseSounds.Length > 0)
        {
            StartCoroutine(PlayRandomNoise());
        }
    }

    private void PlayTitleSound()
    {
        AudioClip soundToPlay = titleSound != null ? titleSound : soundEffectAudioSource.clip;

        if (soundToPlay == null)
        {
            Debug.LogWarning("[TitleSoundScript] 再生する効果音が設定されていません。", this);
        }
        else
        {
            soundEffectAudioSource.PlayOneShot(soundToPlay);
        }
    }

    private void PlayBgm()
    {
        if (bgm == null)
        {
            Debug.LogWarning("[TitleSoundScript] 再生するBGMが設定されていません。", this);
            return;
        }

        bgmAudioSource.clip = bgm;
        bgmAudioSource.volume = 0f;
        bgmAudioSource.Play();
        StartCoroutine(FadeInBgm());
    }

    private IEnumerator FadeInBgm()
    {
        if (bgmFadeInDuration <= 0f)
        {
            bgmAudioSource.volume = bgmVolume;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < bgmFadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            bgmAudioSource.volume = Mathf.Lerp(
                0f,
                bgmVolume,
                Mathf.Clamp01(elapsedTime / bgmFadeInDuration));
            yield return null;
        }

        bgmAudioSource.volume = bgmVolume;
    }

    private IEnumerator PlayRandomNoise()
    {
        while (true)
        {
            float minimumInterval = Mathf.Max(0.1f, Mathf.Min(minimumNoiseInterval, maximumNoiseInterval));
            float maximumInterval = Mathf.Max(minimumInterval, maximumNoiseInterval);
            float waitTime = Random.Range(minimumInterval, maximumInterval);

            yield return new WaitForSecondsRealtime(waitTime);

            AudioClip noise = GetRandomNoise();

            if (noise != null)
            {
                float minimumDuration = Mathf.Max(0.1f, Mathf.Min(minimumNoiseDuration, maximumNoiseDuration));
                float maximumDuration = Mathf.Max(minimumDuration, maximumNoiseDuration);
                float playDuration = Random.Range(minimumDuration, maximumDuration);

                noiseAudioSource.clip = noise;
                noiseAudioSource.volume = noiseVolume;
                noiseAudioSource.Play();

                yield return new WaitForSecondsRealtime(playDuration);

                noiseAudioSource.Stop();
            }
        }
    }

    private AudioClip GetRandomNoise()
    {
        int startIndex = Random.Range(0, noiseSounds.Length);

        for (int offset = 0; offset < noiseSounds.Length; offset++)
        {
            AudioClip noise = noiseSounds[(startIndex + offset) % noiseSounds.Length];

            if (noise != null)
            {
                return noise;
            }
        }

        return null;
    }
}
