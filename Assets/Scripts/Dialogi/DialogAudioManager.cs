using UnityEngine;
using System.Collections;

public class DialogAudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip currentClip;
    private Coroutine stopAudioRoutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("[DialogAudioManager] Brak AudioSource na tym obiekcie!");
        }
    }

    /// <summary>
    /// Odtwarza audio clip przez określony czas
    /// </summary>
    public void PlayAudioClip(AudioClip clip, float duration)
    {
        if (audioSource == null)
        {
            Debug.LogError("[DialogAudioManager] AudioSource nie znaleziony!");
            return;
        }

        StopCurrentAudio();

        currentClip = clip;

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            // Debug.Log("[DialogAudioManager] Playing clip: " + clip.name);

            stopAudioRoutine = StartCoroutine(StopAudioAfterDelay(duration));
        }
    }

    /// <summary>
    /// Zatrzymuje aktualne audio natychmiast
    /// </summary>
    public void StopCurrentAudio()
    {
        if (stopAudioRoutine != null)
        {
            StopCoroutine(stopAudioRoutine);
            stopAudioRoutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        currentClip = null;
    }

    private IEnumerator StopAudioAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        stopAudioRoutine = null;
    }
}
