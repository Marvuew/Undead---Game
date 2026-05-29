using UnityEngine;
using UnityEngine.UI;

public class PauseMenuAutoSubscriber : MonoBehaviour
{
    public Button Resume;
    public Button Quit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.instance != null)
        {
            Resume.onClick.AddListener(GameManager.instance.Resume);
            Quit.onClick.AddListener(GameManager.instance.Quit);
            print("DOne");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
