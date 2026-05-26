using Assets.Scripts.GameScripts;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public List<Undead> undeadDatabase = new List<Undead>();

    [SerializeField] private GameObject pauseMenu;

    public bool isConfrontationTime;

    

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            MajorDecisions.Clear();
            Debug.Log("Cleared Major Decisions");
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneNames.PriestHouse.ToString());
    }

    public void ToggleActive(GameObject target)
    {
        target.SetActive(!target.activeSelf);
    }

    public void Pause()
    {
        Player.Instance.interacting = true;
        pauseMenu.SetActive(true);
    }

    public void Resume()
    {
        Player.Instance.interacting = false;
        pauseMenu.SetActive(false);
    }

    public HashSet<string> MajorDecisions = new HashSet<string>();
}

public enum SceneNames
{
    OpenWorld,
    PriestHouse,
    Dhamphir_House,
    Church,
    MainMenu
}
