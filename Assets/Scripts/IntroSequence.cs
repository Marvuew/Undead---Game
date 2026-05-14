using Assets.Scripts.GameScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;

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
    public Button skipIntroButton;

    [Header("For Moving Undead Portraits")]
    public Transform LeftPanel;
    public Transform RightPanel;

   

    void Start()
    {
        if (undeadPrefab == null) Debug.LogWarning("undeadPrefab is null");
        if (openingDialogue == null) Debug.LogWarning("openingDialogue is null");
        if (mainMenuUI == null) Debug.LogWarning("mainMenuUI is null");
        StartPanelAnimation();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    // called second
    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called third
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu")
        {
            return;
        }
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        INTROUI.SetActive(scene.name == SceneNames.MainMenu.ToString());
        mainMenuUI.SetActive(scene.name == SceneNames.MainMenu.ToString());
        LOGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 1000); // Hardcoded to avoid failure
        LOGO.SetActive(false);
        LeftPanel.gameObject.SetActive(true);
        RightPanel.gameObject.SetActive(true);
        StartPanelAnimation();
        DialogueGraphManager.instance.currentInteractable = null;
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

        StartCoroutine(FadeInSkipButton());
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
        skipIntroButton.gameObject.SetActive(false);
        StartCoroutine(LOGO.GetComponent<LOGO_Animation>().ScaleOverTime()); // TAKES 5 SECONDS

        yield return new WaitForSeconds(2f);

        WorldFade.Instance.StartSceneTransitionAndToggleGameObject(SceneNames.Dhamphir_House.ToString(), 2f, Color.black, INTROUI);
    }

    public IEnumerator FadeInSkipButton()
    {
        skipIntroButton.gameObject.SetActive(true);
        // Lerp the Alpha of the button - Lil juice...
        float timer = 0f;
        CanvasGroup canvasGroup = skipIntroButton.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        while (timer < 2f)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, timer / 2f);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public void SkipIntro()
    {
        skipIntroButton.gameObject.SetActive(false);
        DialogueGraphManager.instance.EndDialogue();

    }

}