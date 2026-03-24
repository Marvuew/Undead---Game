using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame() { SceneManager.LoadScene("Game"); }
    public void Quit() { Application.Quit(); }
    public void Pause() { Time.timeScale = 0; }
    public void Resume() { Time.timeScale = 1; }

}
