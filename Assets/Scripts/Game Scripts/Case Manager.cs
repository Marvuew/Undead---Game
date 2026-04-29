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
    [SerializeField] private GameObject interactablePrefab;
    public Case currentCase;
    [NonSerialized]
    public HashSet<Clue> cluesfound = new HashSet<Clue>();

    [SerializeField]
    List<Undead> undeadDatabase = new List<Undead>();

    public List<Case> allCases = new List<Case>();
    private List<GameObject> activeInteractables = new List<GameObject>();

    [NonSerialized]
    public Dictionary<Clue, List<string>> clueDescriptions = new Dictionary<Clue, List<string>>();

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
        string currentSceneName = SceneManager.GetActiveScene().name;

        foreach (InteractableScriptableObject data in currentCase.interactables)
        {
            if (data.homeScene.ToString() != currentSceneName) continue;

            GameObject newInteractable = Instantiate(interactablePrefab, data.position, Quaternion.identity);

            SpriteRenderer sr = newInteractable.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = data.interactableSprite;
                sr.sortingLayerName = data.sortingLayerName; // SET THE LAYER
                sr.sortingOrder = data.orderInLayer;         // SET THE ORDER
                Debug.Log($"Spawning {data.name} at Order: {data.orderInLayer}");
            }

            RuntimeInteractable script = newInteractable.GetComponent<RuntimeInteractable>();
            script.interactableData = data;
            script.interactableType = data.interactableType;
            script.dialogueGraph = data.dialogue;
            script.interactableClue = data.clue;

            activeInteractables.Add(newInteractable);
        }
    }
    public void InitialClueFound(Clue clueFound)
    {
        if (!cluesfound.Contains(clueFound))
        {
            cluesfound.Add(clueFound);
            AddClueDescription(clueFound, clueFound.initialDescription);
            StartCoroutine(AudioManager.instance.QueueClueFoundSound());
            if (clueFound.undeadTypes.Count > 0)
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
        }
        else
        {
            return;
        }
    } //updates undead tally and clues found in book 

    public void TransitionToSelectScene()
    {
        var selectScene = FindAnyObjectByType<CulpritSelectionScript>();
        StartCoroutine(selectScene.SetupSelectScene(undeadDatabase));
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
        foreach (var interactable in activeInteractables)
        {
            Destroy(interactable);
        }
    }

    public void AddClueDescription(Clue clue, string description)
    {
        if (!clueDescriptions.ContainsKey(clue)) // IF CLUE IS NOT IN THE DICTIONARY CREATE A LIST
        {
            clueDescriptions[clue] = new List<string>();

            if (!cluesfound.Contains(clue)) // ALSO ADD IT TO THE CLUESFOUND IF NOT FOUND YET
            {
                cluesfound.Add(clue);
            }
        }

        List<string> descriptions = clueDescriptions[clue]; // GET REFERENCE TO LIST<STRING>

        if (descriptions.Contains(description))
        {
            Debug.LogWarning($"Description for {clue.name} already exists.");
            return;
        }

        descriptions.Add(description); // ADD THE DESCRIPTION TO THE LIST
        Debug.Log($"Added unique description to {clue.name}. Total descriptions: {descriptions.Count}");
    }

    public void ClueInfoUpdated(Clue clue, string description, List<UndeadType> types)
    {
        AddClueDescription(clue, description);
        StartCoroutine(AudioManager.instance.QueueClueFoundSound());
        if (clue.undeadTypes.Count > 0)
        {
            foreach (UndeadType type in types)
            {
                if (undeadTally.ContainsKey(type))
                    undeadTally[type]++;
            }
        }
        NecroLexiconUI.Instance.UpdateCluesList();
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
