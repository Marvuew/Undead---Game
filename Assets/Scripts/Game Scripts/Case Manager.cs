using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class CaseManager : MonoBehaviour
{
    public static CaseManager Instance { get; private set; }

    [Header("Case SetUp")]
    [SerializeField] private GameObject cluePrefab;
    public Case currentCase;
    [NonSerialized]
    public HashSet<Clue> cluesfound = new HashSet<Clue>();

    [SerializeField]
    List<Undead> UndeadDatabase = new List<Undead>();

    public List<Case> allCases = new List<Case>();
    private List<GameObject> ActiveInteractables = new List<GameObject>();

    [Header("Temporary")]
    public UndeadType undeadChosen;

    bool isActive = false;

    //Clues pointing to each given undead
    private Dictionary<UndeadType, int> undeadTally = 
    Enum.GetValues(typeof(UndeadType))
        .Cast<UndeadType>()
        .ToDictionary(value => value, value => 0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    } //Ensuring singleton pattern
    public int GetClueCount(UndeadType undead) 
    { 
        return undeadTally.TryGetValue(undead, out int count) ? count : 0;
    } // returns the tally/cluesfound for the given undead creature

    public void SetUpClues()
    {
        ClearActiveClues();
        if (isActive)
        {
            Debug.LogWarning("Case is already set up");
            return;
        }
        isActive = true;
        foreach (Clue clue in currentCase.clues)
        {
            Debug.Log("Instantiating: " + clue);
            if (clue.sceneName.ToString() != SceneManager.GetActiveScene().name) continue;
            GameObject newClue = Instantiate(cluePrefab, clue.position, Quaternion.identity);
            newClue.GetComponent<interactable>().clue = clue;
            newClue.GetComponent<interactable>().dialogueGraph = clue.dialogueGraph;
            newClue.GetComponent<SpriteRenderer>().sprite = clue.sprite;
            ActiveInteractables.Add(newClue);
        }
    }  // spawning in clues used maybe in the future
    public void OnClueFound(Clue clueFound)
    {
        if (cluesfound.Contains(clueFound))
        {
            print("Clue has already been found");
            return;
        }
        Debug.Log("Clue=" + (clueFound));
        cluesfound.Add(clueFound);
        StartCoroutine(AudioManager.instance.QueueClueFoundSound());
        if(clueFound.undeadTypes.Count > 0) 
        {
            foreach (UndeadType type in clueFound.undeadTypes) 
            {
                if (undeadTally.ContainsKey(type))
                    undeadTally[type]++;
            }
            Debug.Log("Updated tally");
        }
        Debug.Log("calling book update");
        NecroLexiconUI.Instance.UpdateCluesList();
    } //updates undead tally and clues found in book 

    public void TransitionToSelectScene()
    {
        var selectScene = FindAnyObjectByType<CulpritSelectionScript>();
        StartCoroutine(selectScene.SetupSelectScene(UndeadDatabase));
    }

    public void TemporaryAddTallyToSuspect()
    {
        undeadTally[undeadChosen]++;
    }

    public void LoadNextCase()
    {
        ClearActiveClues();
        cluesfound.Clear();
        int currentCaseIndex = allCases.IndexOf(currentCase);
        Debug.Log(currentCaseIndex);
        Debug.Log(allCases[currentCaseIndex]);
        if (currentCaseIndex++ >= allCases.Count)
        {
            Debug.LogWarning("Youve reached the last case");
            return;
        }
        isActive = false;
        currentCase = allCases[currentCaseIndex++];
        SetUpClues();
    }

    public void ClearActiveClues()
    {
        foreach (var interactable in ActiveInteractables)
        {
            Destroy(interactable);
        }
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
