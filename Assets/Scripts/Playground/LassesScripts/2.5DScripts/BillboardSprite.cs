using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        transform.forward = mainCam.transform.forward;
    }
}