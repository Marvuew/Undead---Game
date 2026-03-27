using Assets.Scripts.GameScripts;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    [SerializeField] private GameObject pauseMenu;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void StartGame() { SceneManager.LoadScene("Game"); }
    public void MainMenu() { SceneManager.LoadScene("Main Menu"); }

    public void Quit() { Application.Quit(); }
    public void Pause() 
    {
        Player.instance.interacting = true; 
        pauseMenu.SetActive(true);
    }
    public void Resume() 
    {
        Player.instance.interacting = false;
        pauseMenu.SetActive(false);
    }

}
