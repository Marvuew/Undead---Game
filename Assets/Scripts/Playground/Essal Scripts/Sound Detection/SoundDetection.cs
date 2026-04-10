using UnityEngine;
using UnityEngine.InputSystem;
public class SoundDetection : MonoBehaviour
{
    [Header("Insert Stuff")]
    [SerializeField] float DistanceFromSoundObject;
    [SerializeField] MousePos mouse;
    [SerializeField] AudioClip sound;

    AudioSource audioSource;
    AudioLowPassFilter lowPassFilter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = sound;
        lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
    }

    void Update()
    {
        if (!mouse.enabled)
        {
            audioSource.Stop();
            return;
        }

        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        float distance = Vector2.Distance(mouseWorld, transform.position);

        RefreshVolume(distance);
    }

    void RefreshVolume(float distance)
    {
        if (distance < DistanceFromSoundObject && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (distance >= DistanceFromSoundObject && audioSource.isPlaying)
        {
            audioSource.Stop();
            return;
        }
        
        if (audioSource.isPlaying)
        {
            float normalized = Mathf.InverseLerp(DistanceFromSoundObject, 0f, distance);
            float curved = Mathf.Pow(normalized, 2.5f);
            audioSource.volume = Mathf.Lerp(audioSource.volume, curved, Time.deltaTime * 5f);
            audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, curved);
            lowPassFilter.cutoffFrequency = Mathf.Lerp(500f, 22000f, curved);
        }
    }
}
