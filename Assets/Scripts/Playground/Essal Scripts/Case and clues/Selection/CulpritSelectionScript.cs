using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq.Expressions;
public class CulpritSelectionScript : MonoBehaviour
{
    [Header("Corkboard")]
    public Transform corkBoardContainer;
    public Button culpritButtonPrefab;
    public GameObject corkBoard;
    public Image backGround;
    public ConfrontationScript confrontationScript;

    public IEnumerator SetupSelectScene(List<Suspect> culprits)
    {
        // Clear the buttons first
        ClearCulprits();
        //Then play the animation
        AnimationManager.instance.BlackFadeAnimation();
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false);

        //Handle the Corkboard UI;
        backGround.enabled = true;
        corkBoard.SetActive(true);
        foreach (var culprit in culprits)
        {
            Button button = Instantiate(culpritButtonPrefab, corkBoardContainer);
            button.GetComponent<Image>().sprite = culprit.culpritSprite;
            button.onClick.AddListener(() => HandleCulpritGuess(culprit, button, backGround));
        }
    }

    private void ClearCulprits()
    {
        foreach (Transform child in corkBoardContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void MakeButtonsNotInteractable()
    {
        foreach (Transform child in corkBoardContainer)
        {
            child.GetComponent<Button>().interactable = false;
        }
    }

    public void HandleCulpritGuess(Suspect culprit, Button button, Image selectionGround)
    {
        MakeButtonsNotInteractable();
        bool isCulprit = CaseManager.instance.currentCase.culprit == culprit;
        StartCoroutine(confrontationScript.Manifest(CaseManager.instance.foundClues.Count, isCulprit, corkBoard, culprit, backGround));
    }


    #region Legacy Code

    /*public void CalculateConfrontation(int foundClues, Culprit culprit)
    {
        ConfrontationManager.instance.TransferCulprit(culprit);

        if (foundClues == 1)
        {
            StartCoroutine(ConfrontationManager.instance.Level1Manifestation());
        }
        else if (foundClues == 2)
        {
            StartCoroutine(ConfrontationManager.instance.Level2Manifestation());
        }
        else if (foundClues == 3)
        {
            StartCoroutine(ConfrontationManager.instance.Level3Manifestation());
        }
    }*/

    /*public void HandleGuess(Culprit culprit)
{
    print($"You Found {CaseManager.instance.foundClues.Count} out of {CaseManager.instance.currentCase.clues.Count}");

    if (CaseManager.instance.foundClues.Count == 0)
    {
        Debug.LogWarning("This is not serious, you cant be serious");
    }
    if (CaseManager.instance.currentCase.culprit == culprit)
    {
        CalculateConfrontation(CaseManager.instance.foundClues.Count, CaseManager.instance.currentCase.culprit);
    }
    else
    {
        StartCoroutine(ConfrontationManager.instance.Level0Manifestation());
    }
}*/
    #endregion

}
