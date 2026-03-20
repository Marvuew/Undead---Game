using UnityEngine;
using UnityEngine.UI;

public class VampireVisionManager : MonoBehaviour
{
    public static VampireVisionManager instance;

    private void Awake()
    {
        instance = this;
    }
    public Image VampireVisionImage;
    public bool VampireVisionActive;
    public GameObject VampireVisionEye;

    public void ActivateVampireVision()
    {
        if (VampireVisionActive)
        {
            Debug.Log("Vampire Vision is already active");
            return;
        }
        else
        {
            VampireVisionImage.gameObject.SetActive(true);
            VampireVisionActive = true;
            VampireVisionEye.SetActive(true);
        }
    }

    public void DeactivateVampireVision()
    {
        if (!VampireVisionActive)
        {
            Debug.Log("Vampire vision isnt even active yet");
            return;
        }
        else
        {
            VampireVisionImage.gameObject.SetActive(false);
            VampireVisionEye.SetActive(false);
            VampireVisionActive = false;
            
        }
    }
}
