using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;


public class OutroHandler : MonoBehaviour
{
    [Header("Inspector")]
    //public TextMeshProUGUI outroTextElement;
    public ScrollRect scroll;
    public float panSpeed;
    public Transform endCreditContainer;
    public GameObject textPrefab;
    public Button SkipEndCreditBtn;
    public GameObject EndCreditUI;

    /*Image SuperBlueBloodMoon;
    string outroText;

    private void Awake()
    {
        SuperBlueBloodMoon = GetComponent<Image>();
    }

    public IEnumerator SetupOutro(Undead culprit, int foundClues, bool rightCulprit, Image confrontationBackground)
    {
        confrontationBackground.enabled = false; // DISABLE THE CONFRONTATION BACKGROUND
        EnableOutroUI(); // ENABLE THE UI FOR OUTRO

        CalculateTextOutput(foundClues, rightCulprit, culprit); // CALCULATE TEXT OUTPUT
        StartCoroutine(AnimationManager.instance.TypeWriterEffect(outroText, outroTextElement, 0.05f)); // WRITE THE FINAL ANALYSIS

        yield return new WaitUntil(() => Keyboard.current.spaceKey.wasPressedThisFrame); // WAIT FOR SPACE PRESS

        StopAllCoroutines();
        StartCoroutine(SetupNextCase()); // SETUP THE NEXT CASE
    }

    public IEnumerator SetUpOutro(Undead culprit, int foundClues, bool rightCulprit)
    {
        AnimationManager.instance.BlackFadeAnimation(); // PLAY FADE ANIMATION
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false);
        EnableOutroUI(); // ENABLE THE UI FOR OUTRO
        CalculateTextOutput(foundClues, rightCulprit, culprit); // CALCULATE TEXT OUTPUT
        StartCoroutine(AnimationManager.instance.TypeWriterEffect(outroText, outroTextElement, 0.05f)); // WRITE THE FINAL ANALYSIS
        yield return new WaitUntil(() => Keyboard.current.spaceKey.wasPressedThisFrame); // WAIT FOR SPACE PRESS
        StopAllCoroutines();
        StartCoroutine(SetupNextCase()); // SETUP THE NEXT CASE
    }

    public void CalculateTextOutput(int foundClues, bool rightSuspect, Undead suspect)
    {
        if (foundClues == 0)
        {
            outroText = "Do better...seriously";
        }
        else if (foundClues > 0)
        {
            outroText = $"You found {foundClues} clues which led you to believe that {suspect.name} was the right culprit. This guess was {rightSuspect}";
        }
    }

    public IEnumerator SetupNextCase()
    {
        AnimationManager.instance.BlackFadeAnimation(); // PLAY FADE ANIMATION
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false);

        UIManager.instance.VisibleUI(); // ?? MAKES THE UI VISIBLE??
        DisableOutroUI();
        Debug.Log("Setting up next case");
        StartCoroutine(CaseManager.Instance.ReturnToMainMenu());
    }

    public void EnableOutroUI() // ENABlE THE UI FOR OUTRO
    {
        SuperBlueBloodMoon.enabled = true;
        outroTextElement.enabled = true;
    }

    public void DisableOutroUI() // DISBALE THE UI FOR OUTRO
    {
        SuperBlueBloodMoon.enabled = false;
        outroTextElement.enabled = false;
    }*/

    public IEnumerator EndCreditsPan()
    {
        SpawnMajorDecisions();
        StartCoroutine(FadeInSkipButton());
        Debug.Log("Panning End Credits");
        gameObject.SetActive(true);

        scroll.content.anchoredPosition = new Vector2(scroll.content.anchoredPosition.x, 0);

        float contentHeight = scroll.content.rect.height;

        float viewportHeight = scroll.viewport.rect.height;
        float targetY = contentHeight + viewportHeight;

        while (scroll.content.anchoredPosition.y < targetY)
        {
            float newY = scroll.content.anchoredPosition.y + (panSpeed * Time.deltaTime);
            scroll.content.anchoredPosition = new Vector2(scroll.content.anchoredPosition.x, newY);
            yield return null;
        }

        scroll.content.anchoredPosition = new Vector2(scroll.content.anchoredPosition.x, targetY);
        Debug.Log("Panning complete.");

        WorldFade.Instance.StartSceneTransition(SceneNames.MainMenu.ToString(), 2f, Color.white);
        yield return new WaitForSeconds(2f);
        EndCreditUI.SetActive(false);
    }

    public void SpawnMajorDecisions()
    {
        if (GameManager.instance.MajorDecisions.Count == 0)
        {
            GameManager.instance.MajorDecisions.Add("Neurogenesis is the process by which nervous system cells, the neurons, are produced by neural stem cells (NSCs).");
            GameManager.instance.MajorDecisions.Add("This occurs in all species of animals except the porifera (sponges) and placozoans.[2]");
            GameManager.instance.MajorDecisions.Add("Types of NSCs include neuroepithelial cells (NECs), radial glial cells (RGCs), basal progenitors (BPs), intermediate neuronal precursors (INPs), subventricular zone astrocytes, and subgranular zone radial astrocytes, among others.[2]");
            GameManager.instance.MajorDecisions.Add("Neurogenesis is most active during embryonic development and is responsible for producing all the various types of neurons of the organism, but it continues throughout adult life in a variety of organisms.[2");
            GameManager.instance.MajorDecisions.Add("Once born, neurons do not divide (see mitosis), and many will live the lifespan of the animal, except under extraordinary and usually pathogenic circumstances.[3]");
            GameManager.instance.MajorDecisions.Add("During embryonic development, the mammalian central nervous system (CNS; brain and spinal cord) is derived from the neural tube, which contains NSCs that will later generate neurons.[3]");
            GameManager.instance.MajorDecisions.Add("However, neurogenesis doesn't begin until a sufficient population of NSCs has been achieved.");
            GameManager.instance.MajorDecisions.Add("These early stem cells are called neuroepithelial cells (NEC)s, but soon take on a highly elongated radial morphology and are then known as radial glial cells (RGC)s.[3");
        }
        foreach (string decision in GameManager.instance.MajorDecisions)
        {
            var text = Instantiate(textPrefab, endCreditContainer);
            text.GetComponent<TextMeshProUGUI>().text = decision;
        }
        Debug.Log("Spawned All Sentences");
    }

    public void SkipEndCredits()
    {
        float contentHeight = scroll.content.rect.height;

        float viewportHeight = scroll.viewport.rect.height;
        float targetY = contentHeight + viewportHeight;
        scroll.content.anchoredPosition = new Vector2(scroll.content.anchoredPosition.x, targetY);
    }

    public IEnumerator FadeInSkipButton()
    {
        SkipEndCreditBtn.gameObject.SetActive(true);
        // Lerp the Alpha of the button - Lil juice...
        float timer = 0f;
        CanvasGroup canvasGroup = SkipEndCreditBtn.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        while (timer < 5f)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, timer / 2f);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
}
