using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class CaseManager : MonoBehaviour
{
    public static CaseManager instance { get; private set; }

    [Header("Case SetUp")]
    [SerializeField] private GameObject cluePrefab;
    public Case currentCase;

    private Dictionary<Undead, int> undeadTallly;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (Undead value in (Undead[])Enum.GetValues(typeof(Undead)))
        {
            undeadTallly[value] = 0;
        }
    }

    public void SetUpClues()
    {
        foreach (Clue clue in currentCase.clues)
        {
            GameObject newClue = Instantiate(cluePrefab, clue.position, Quaternion.identity);
            newClue.GetComponent<interactable>().clue = clue;
        }
    }
    public void ClueFound(Clue clueFound)
    {
        foreach (Undead type in clueFound.undeadTypes) 
        {
            if (undeadTallly.ContainsKey(type))
                undeadTallly[type]++;
        }
        NecroLexiconScript.instance.UpdateClueList(clueFound);
    }

}
/*
public static CaseManager instance;
private void Awake()
{
    instance = this;
    LoadNextCase();
}

[Header("Case To Handle")]
public Case currentCase;

[Header("Databases")]
public List<Case> caseDatabase = new List<Case>();
public List<Culprit> CulpritDatabase = new List<Culprit>();
public List<Item> itemDatabase = new List<Item>();

[Header("Found clues")]
public List<Clue> foundClues = new List<Clue>();

public Transform clueContainer;
public GameObject cluePrefab;

public CulpritSelectionScript culpritSelection;
int caseIndex = 0;
public void AddClue(Clue clue)
{
    foundClues.Add(clue);
}

public void LoadNextCase()
{
    foundClues.Clear();

    Debug.Log("Loading next case");
    if (caseIndex >= caseDatabase.Count)
    {
        Debug.LogWarning("No more cases!");
        return;
    }
    else
    {
        currentCase = caseDatabase[caseIndex];
        caseIndex++;
    }
    InstantiateClues();
}

public void InstantiateClues()
{
    ClearClues();
    foreach (var clue in currentCase.clues)
    {
        GameObject _clue = Instantiate(cluePrefab, clueContainer);
        _clue.GetComponent<RuntimeClue>().clue = clue;
        _clue.GetComponent<Image>().sprite = clue.clueSprite;
    }
}

public void ClearClues()
{
    foreach (Transform child in clueContainer)
    {
        Destroy(child.gameObject);
    }
}

public void EndDay()
{
    UIManager.instance.TransparentUI();
    StartCoroutine(culpritSelection.SetupSelectScene(CulpritDatabase));
}
}
*/
