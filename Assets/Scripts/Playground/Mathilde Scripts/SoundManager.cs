using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip openBookSound;
    public AudioClip closeBookSound;
    public AudioClip[] pageTurnSounds;

    public void PlayCloseBookSound()
    {
        sfxSource.PlayOneShot(closeBookSound);
    }

    public void PlayOpenBookSound()
    {
        sfxSource.PlayOneShot(openBookSound);
    }

    public void PlayPageTurnSound()
    {
        if (pageTurnSounds != null && pageTurnSounds.Length > 0)
        {
            int index = Random.Range(0, pageTurnSounds.Length);
            sfxSource.PlayOneShot(pageTurnSounds[index]);
        }
    }

}
