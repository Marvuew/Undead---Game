using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Android;
using UnityEngine.InputSystem;


public class CaseOutroScript : MonoBehaviour
{
    [Header("Inspector")]
    public TextMeshProUGUI outroTextElement;

    Image SuperBlueBloodMoon;
    string outroText;

    private void Awake()
    {
        SuperBlueBloodMoon = GetComponent<Image>();
    }

    public IEnumerator SetupOutro(Suspect culprit, int foundClues, bool rightCulprit, Image confrontationBackground)
    {
        confrontationBackground.enabled = false;
        // Setup Outro UI
        EnableOutroUI();

        //Calculate and write the text
        CalculateTextOutput(foundClues, rightCulprit, culprit);
        StartCoroutine(AnimationManager.instance.TypeWriterEffect(outroText, outroTextElement, 0.05f));

        yield return new WaitUntil(() => Mouse.current.leftButton.wasPressedThisFrame);

        StartCoroutine(SetupNextCase());

    }

    public void CalculateTextOutput(int foundClues, bool rightCulprit, Suspect culprit)
    {
        if (foundClues == 0)
        {
            outroText = "Do better...seriously";
        }
        else if (foundClues > 0)
        {
            outroText = $"You found {foundClues} clues which led you to believe that {culprit.culpritName} was the right culprit. This guess was {rightCulprit}";
        }
    }

    public IEnumerator SetupNextCase()
    {
        AnimationManager.instance.BlackFadeAnimation();
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false);
        UIManager.instance.VisibleUI();

        DisableOutroUI();

        CaseManager.instance.LoadNextCase();
    }

    public void EnableOutroUI()
    {
        SuperBlueBloodMoon.enabled = true;
        outroTextElement.enabled = true;
    }

    public void DisableOutroUI()
    {
        Debug.Log("Disabling");
        SuperBlueBloodMoon.enabled = false;
        outroTextElement.enabled = false;
    }
}
