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

    List<Clue> foundClues = new List<Clue>();
    
    public void AddClue(Clue clue)
    {
        foundClues.Add(clue);
    }

    public void EndDay()
    {
        print($"You Found {foundClues.Count} out of {currentCase.clues.Count}");
        CalculateConfrontation(foundClues.Count, currentCase.culprit);
    }

    public void CalculateConfrontation(int foundClues, Culprit culprit)
    {
        ConfrontationManager.instance.TransferCulprit(culprit);

        if (foundClues == 0)
        {
            StartCoroutine(ConfrontationManager.instance.Level0Manifestation());
        }
        else if (foundClues == 1)
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
    }
}
