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
    }

    private void Start()
    {
        PlayTitleSound();
        PlayBgm();
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
}
