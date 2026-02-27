using UnityEngine;
using UnityEngine.InputSystem;

public class SoundDetection : MonoBehaviour
{
    [Header("Insert Stuff")]
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _audioClip;
    [SerializeField] AudioLowPassFilter _lowPassFilter;
    [SerializeField] float DistanceFromSoundObject;

    Vector3 screenPos;
    Vector3 worldPos;
    float distance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.GetComponent<SoundDetection>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        screenPos = Mouse.current.position.ReadValue();
        screenPos.z = Camera.main.nearClipPlane;
        worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        distance = Vector2.Distance(worldPos, transform.position);
        RefreshVolume(distance);
    }

    void RefreshVolume(float distance)
    {
        if (distance < DistanceFromSoundObject && !_audioSource.isPlaying)
        {
            _audioSource.clip = _audioClip;
            _audioSource.volume = 0;
            _audioSource.loop = true;
            _audioSource.Play();
        }
        else if (distance >= DistanceFromSoundObject && _audioSource.isPlaying)
        {
            _audioSource.Stop();
            return;
        }
        
        if (_audioSource.isPlaying)
        {
            float normalized = Mathf.InverseLerp(DistanceFromSoundObject, 0f, distance);
            float curved = Mathf.Pow(normalized, 2.5f);
            _audioSource.volume = Mathf.Lerp(_audioSource.volume, curved, Time.deltaTime * 5f);
            _audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, curved);
            _lowPassFilter.cutoffFrequency = Mathf.Lerp(500f, 22000f, curved);
        }
    }
}
