using Assets.Scripts.GameScripts;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
public class SelectionHandler : MonoBehaviour
{
    public RuntimeDialogueGraph confrontationTimeInitDialogueGraph;

    [Header("Corkboard")]
    public Transform corkBoardContainer;
    public Button culpritButtonPrefab;
    public GameObject corkBoard;
    public ConfrontationHandler confrontationScript;
    public TextMeshProUGUI culpritNameText;

    public IEnumerator SetupSelectScene(List<Undead> culprits)
    {
        Debug.Log("Setting up Select Scene");
        ClearCulprits(); // CLEAR BUTTONS
        WorldFade.Instance.StartSceneTransition(SceneNames.Dhamphir_House.ToString(), 2f, Color.black, new Vector3(0.03000021f, 1.06f, 0)); // Transistions to the scene of the culprit.

        // WHY AM I STILL SPAWNING AT THE BED????

        yield return new WaitUntil(() => !WorldFade.Instance.isSceneTransitioning2); // Waits until the transition is done.
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == SceneNames.Dhamphir_House.ToString()); // Waits until the scene is actually loaded, just to be sure.
        GameManager.instance.isConfrontationTime = true;
        DialogueGraphManager.instance.StartDialogue(confrontationTimeInitDialogueGraph);

    }

    public void SetupCorkBoard(List<Undead> culprits)
    {
        // HANDLE CORKBOARD UI
        Debug.Log("Setting up corkboard");
        if (culprits == null) Debug.LogWarning("Culprits list is null?");
        corkBoard.SetActive(true);
        ClearCulprits();
        foreach (var suspect in culprits)
        {
            Button button = Instantiate(culpritButtonPrefab, corkBoardContainer);
            button.GetComponent<CulpritButtonMouseOverHandler>().dealtUndead = suspect;
            button.GetComponent<Image>().sprite = suspect.cardSprite;
            button.onClick.AddListener(() => HandleCulpritGuess(suspect, button));
            GameManager.instance.isConfrontationTime = false;
        }
    }

    public void CloseCorkBoard()
    {
        corkBoard.SetActive(false);
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

    public void HandleCulpritGuess(Undead suspect, Button button) // METHOD FOR CALCULATING CULPRIT GUESS
    {
        MakeButtonsNotInteractable();
        bool isCulprit = CaseManager.Instance.currentCase.culprit == suspect;
        StartCoroutine(confrontationScript.Confrontation(CaseManager.Instance.GetClueCount(suspect.undeadType), isCulprit, corkBoard, suspect)); // SETTING UP THE CONFRONTATION SCREEN
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
