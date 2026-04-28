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

    public IEnumerator SetupSelectScene(List<Undead> culprits)
    {
        ClearCulprits(); // CLEAR BUTTONS
        AnimationManager.instance.BlackFadeAnimation(); // FADE ANIMATION
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false); // WAIT TILL ITS DONE

        // HANDLE CORKBOARD UI
        backGround.enabled = true;
        corkBoard.SetActive(true);
        foreach (var suspect in culprits)
        {
            Button button = Instantiate(culpritButtonPrefab, corkBoardContainer);
            button.GetComponent<Image>().sprite = suspect.undeadSprite;
            button.onClick.AddListener(() => HandleCulpritGuess(suspect, button, backGround));
        }
    }

    private void ClearCulprits() // HELPER METHOD FOR CLEARING THE CORKBOARD BUTTONS
    {
        foreach (Transform child in corkBoardContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void MakeButtonsNotInteractable() // HELPER METHOD FOR MAKING THE BUTTONS INTERACTABLE
    {
        foreach (Transform child in corkBoardContainer)
        {
            child.GetComponent<Button>().interactable = false;
        }
    }

    public void HandleCulpritGuess(Undead suspect, Button button, Image selectionGround) // METHOD FOR CALCULATING CULPRIT GUESS
    {
        MakeButtonsNotInteractable();
        bool isCulprit = CaseManager.Instance.currentCase.culprit == suspect;
        StartCoroutine(confrontationScript.Manifest(CaseManager.Instance.GetClueCount(suspect.undeadType), isCulprit, corkBoard, suspect, backGround)); // SETTING UP THE CONFRONTATION SCREEN
        Debug.Log($"You chose {suspect} and clues found for chosen suspect = {CaseManager.Instance.GetClueCount(suspect.undeadType)}, that guess was {isCulprit}");
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
