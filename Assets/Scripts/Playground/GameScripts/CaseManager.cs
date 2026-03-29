using Assets.Scripts.Playground.GameScripts;
using UnityEngine;

public class CaseManager : MonoBehaviour
{
    [SerializeField] private GameObject cluePrefab;
    public Case currentCase;
    public static CaseManager instance {get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
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
        Debug.Log("CLUE FOUND : " + clueFound.description);
        //Necrolexicon.instance.updatebook(clueFound.description);
        //Necrolexicon.instance.tallySuspects(clueFound.undeadTypes);
    }
}
