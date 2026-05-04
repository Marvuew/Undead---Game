using Assets.Scripts.GameScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroSequence : MonoBehaviour
{
    [Header("Start Game Animation")]
    public GameObject undeadPrefab;
    public float sinFloat = 5f;
    public bool isRunning = false;
    public RuntimeDialogueGraph openingDialogue;
    public GameObject mainMenuUI;
    public GameObject LOGO;
    public float ratio = 2f;
    public GameObject INTROUI;

    [Header("For Moving Undead Portraits")]
    public Transform LeftPanel;
    public Transform RightPanel;

    void Start()
    {
        if (undeadPrefab == null) Debug.LogWarning("undeadPrefab is null");
        if (openingDialogue == null) Debug.LogWarning("openingDialogue is null");
        if (mainMenuUI == null) Debug.LogWarning("mainMenuUI is null");

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartPanelAnimation()
    {
        if (Keyboard.current != null && !Keyboard.current.enabled)
            InputSystem.EnableDevice(Keyboard.current);

        if (Player.Instance != null)
            Player.Instance.interacting = false;

        List<int> indices = new List<int>(); // CREATE A SHUFFLED LIST OF INDICIES
        for (int i = 0; i < CaseManager.Instance.undeadDatabase.Count; i++) indices.Add(i);

        for (int i = 0; i < indices.Count; i++) // FISHER YATES SHUFFLE
        {
            int temp = indices[i];
            int randomIndex = Random.Range(i, indices.Count);
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;
        }

        foreach (int idx in indices) // Instantiate for the left panel
        {
            var undead = CaseManager.Instance.undeadDatabase[idx];
            GameObject go = Instantiate(undeadPrefab, LeftPanel);
            go.GetComponent<Image>().sprite = undead.cardSprite;
        }

        for (int i = 0; i < indices.Count; i++) // FISHER YATES SHUFFLE SO THEY ARENT IDENTICAL
        {
            int temp = indices[i];
            int randomIndex = Random.Range(i, indices.Count);
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;
        }

        foreach (int idx in indices) // Instantiate for the right panel
        {
            var undead = CaseManager.Instance.undeadDatabase[idx];
            GameObject go = Instantiate(undeadPrefab, RightPanel);
            go.GetComponent<Image>().sprite = undead.cardSprite;
        }
    }

    public IEnumerator HandleIntroDialogue()
    {
        mainMenuUI.SetActive(false);
        // Handle Dialogue
        DialogueGraphManager.instance.gameObject.SetActive(true);



        if (DialogueGraphManager.instance.DialoguePanel != null)
            DialogueGraphManager.instance.DialoguePanel.SetActive(true);

        DialogueGraphManager.instance.StartDialogue(openingDialogue);

        yield return new WaitUntil(() => !DialogueGraphManager.instance.isDialogueRunning);

        if (Player.Instance != null)
            Player.Instance.interacting = false;

        LeftPanel.gameObject.SetActive(false);
        RightPanel.gameObject.SetActive(false);

        LOGO.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (Player.Instance != null)
            Player.Instance.interacting = false;

        WorldFade.Instance.StartSceneTransitionAndStayBlack(SceneNames.Dhamphir_House.ToString(), 2f, Color.black);
        yield return new WaitUntil(() => !WorldFade.Instance.isSceneTransitioning2);
        INTROUI.SetActive(false);
    }
}