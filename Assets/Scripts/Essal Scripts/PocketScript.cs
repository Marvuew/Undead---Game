using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PocketScript : MonoBehaviour
{
    [SerializeField] GameObject Ear;
    [SerializeField] GameObject soundObject;
    [SerializeField] float SizeIncreaseOnHover;

    Vector3 _originalPos;

    MousePos _mouse;
    SoundDetection _soundDetection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _originalPos = Ear.transform.position;
        _mouse = Ear.GetComponent<MousePos>();
        _soundDetection = soundObject.GetComponent<SoundDetection>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseEnter()
    {
        if (Ear == null)
        {
            return;
        }
        Ear.transform.localScale += Vector3.one * SizeIncreaseOnHover;
    }

    private void OnMouseExit()
    {
        Ear.transform.localScale -= Vector3.one * SizeIncreaseOnHover;
    }

    private void OnMouseUp()
    {
        _mouse.enabled = !_mouse.enabled;
        _soundDetection.enabled = !_soundDetection.enabled;

        if (!_mouse.enabled)
        {
            Debug.Log("Returning to OriginalPos");
            Ear.transform.position = _originalPos;
        }
    }
}
