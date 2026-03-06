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
    [HideInInspector] public TextMeshProUGUI speakerTxt;
    [HideInInspector] public TextMeshProUGUI dialogueTxt;
    [HideInInspector] public TextMeshProUGUI optionATxt;
    [HideInInspector] public TextMeshProUGUI optionBTxt;

    public static GameManager instance { get; private set; }

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
        speakerTxt = GameObject.Find("speakerTxt").GetComponent<TextMeshProUGUI>();
        dialogueTxt = GameObject.Find("dialogueTxt").GetComponent<TextMeshProUGUI>();
        optionATxt = GameObject.Find("optionATxt").GetComponent<TextMeshProUGUI>();
        optionBTxt = GameObject.Find("optionBTxt").GetComponent<TextMeshProUGUI>();

        Debug.Log($"speakerTxt is {(speakerTxt != null)}");
        Debug.Log($"dialogueTxt is {(dialogueTxt != null)}");
        Debug.Log($"optionATxt is {(optionATxt != null)}");
        Debug.Log($"optionBTxt is {(optionBTxt != null)}");

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
