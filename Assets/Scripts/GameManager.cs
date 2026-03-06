using Assets.Scripts;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Dialouge")]
    public GameObject dialogueBox;
    public GameObject optionsBox;
    public DialogueNode startNode;
    [HideInInspector] public TextMeshPro speakerTxt;
    [HideInInspector] public TextMeshPro dialogueTxt;
    [HideInInspector] public TextMeshPro optionATxt;
    [HideInInspector] public TextMeshPro optionBTxt;

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
        SetUpUI();
        DialogueManager.SetUpDialogueManager();
    }
    private void SetUpUI() 
    {
        speakerTxt = GameObject.Find("speakerTxt").GetComponent<TextMeshPro>();
        dialogueTxt = GameObject.Find("dialogueTxt").GetComponent<TextMeshPro>();
        optionATxt = GameObject.Find("optionATxt").GetComponent<TextMeshPro>();
        optionBTxt = GameObject.Find("optionBTxt").GetComponent<TextMeshPro>();

        Console.WriteLine($"speakerTxt is {(speakerTxt != null)}");
        Console.WriteLine($"dialogueTxt is {(dialogueTxt != null)}");
        Console.WriteLine($"optionATxt is {(optionATxt != null)}");
        Console.WriteLine($"optionBTxt is {(optionBTxt != null)}");

        dialogueBox.SetActive(false);
        optionsBox.SetActive(false);
    }
    public void startConvo() 
    {
        Console.WriteLine("Starting conversation...");
        DialogueManager.instance.StartDialogue(startNode);
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
