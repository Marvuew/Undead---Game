using Assets.Scripts.GameScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    HashSet<int> selected = new HashSet<int>();

    void Start()
    {
        if (undeadPrefab == null) Debug.LogWarning("undeadPrefab is null");
        if (openingDialogue == null) Debug.LogWarning("openingDialogue is null");
        if (mainMenuUI == null) Debug.LogWarning("mainMenuUI is null");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartPanelAnimation()
    {

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

    public IEnumerator StartIntroDialogue()
    {
        mainMenuUI.SetActive(false); // Deactivate the start buttons
        DialogueGraphManager.instance.StartDialogue(openingDialogue); // Start dialogue
        yield return new WaitUntil(() => !DialogueGraphManager.instance.isDialogueRunning); //Wait till its done
        LeftPanel.gameObject.SetActive(false); // Deactivate bot panels
        RightPanel.gameObject.SetActive(false);
        LOGO.SetActive(true); // Activate the logo
        yield return new WaitForSeconds(1f);
        WorldFade.Instance.StartSceneTransition(SceneNames.Home.ToString(), 5f, Color.white); // Start the scene transition
        yield return new WaitForSeconds(5f); // Wait till animation is done
        INTROUI.SetActive(false); // deactivate the intro UI after scene transition
    }
}
