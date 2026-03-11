using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PocketScript : MonoBehaviour
{
    [SerializeField] GameObject Ear;
    [SerializeField] float SizeIncreaseOnHover;

    Vector3 _originalPos;

    MousePos _mouse;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _originalPos = Ear.transform.position;
        _mouse = Ear.GetComponent<MousePos>();
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
        TimeManager.Instance.RealTimeMode = !TimeManager.Instance.RealTimeMode;

        if (!_mouse.enabled)
        {
            Debug.Log("Returning to OriginalPos");
            Ear.transform.position = _originalPos;
        }
    }
}
