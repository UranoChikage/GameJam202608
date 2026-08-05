using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TitleSoundScript : MonoBehaviour
{
    [Header("タイトル画面の効果音")]
    [SerializeField] private AudioClip titleSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        AudioClip soundToPlay = titleSound != null ? titleSound : audioSource.clip;

        if (soundToPlay == null)
        {
            Debug.LogWarning("[TitleSoundScript] 再生する効果音が設定されていません。", this);
            return;
        }

        audioSource.PlayOneShot(soundToPlay);
    }
}
