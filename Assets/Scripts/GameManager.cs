using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Dialouge")]
    public TextMeshPro speakerTxt;
    public TextMeshPro dialougeTxt;
    public TextMeshPro optionATxt;
    public TextMeshPro optionBTxt;
    public GameObject dialougeBox;
    public GameObject optionsBox;
    public static GameManager instance { get; private set; }
    private GameManager() { }

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame() 
    {
        Console.WriteLine("Starting Game...");
        SceneManager.LoadScene("Game");
    }
    public void QuitGame()
    {
        Console.WriteLine("Quiting Game...");
        Application.Quit();
    }
}
