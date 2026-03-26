using UnityEngine;
using System.Collections.Generic;

public class CaseManager : MonoBehaviour
{
    public static CaseManager instance;
    private void Awake()
    {
        instance = this;    
    }

    public Case currentCase;
    public CulpritSelectionScript culpritSelection;

    public List<Clue> foundClues = new List<Clue>();
    
    public void AddClue(Clue clue)
    {
        foundClues.Add(clue);
    }

    /*public void HandleGuess(Culprit culprit)
    {
        print($"You Found {foundClues.Count} out of {currentCase.clues.Count}");
        if (foundClues.Count == 0)
        {
            Debug.LogWarning("This is not serious, you cant be serious");
        }
        if (currentCase.culprit == culprit)
        {
            CalculateConfrontation(foundClues.Count, currentCase.culprit);
        }
        else
        {
            StartCoroutine(ConfrontationManager.instance.Level0Manifestation());
        }
    }*/

    public void EndDay()
    {
        Debug.Log("Ending Day");
        StartCoroutine(culpritSelection.CreateCorkBoard(GameManager.instance.CulpritDatabase));
    }

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
}
