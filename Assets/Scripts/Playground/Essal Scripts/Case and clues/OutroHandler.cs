using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Android;
using UnityEngine.InputSystem;


public class OutroHandler : MonoBehaviour
{
    [Header("Inspector")]
    //public TextMeshProUGUI outroTextElement;
    public ScrollRect scroll;
    public float panSpeed;

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
    }
}
