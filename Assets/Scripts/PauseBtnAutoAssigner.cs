using UnityEngine;
using UnityEngine.UI;

public class PauseBtnAutoAssigner : MonoBehaviour
{
    public Button PauseBtn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PauseBtn.onClick.AddListener(GameManager.instance.Pause);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
