using Assets.Scripts.GameScripts;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [SerializeField] private GameObject pauseMenu;

    public bool isConfrontationTime;

    IntroSequence introScript;

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

        introScript = FindAnyObjectByType<IntroSequence>();
        if (introScript == null) Debug.LogWarning("Couldnt find the intro script");
    }

    private void Start()
    {

    }

    public void StartGame()
    {
        if (introScript != null)
            StartCoroutine(introScript.HandleIntroDialogue());
        else Debug.LogWarning("Intro Script was null and therefore couldnt start the panel Animation");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneNames.PriestHouse.ToString());
    }

    public void ToggleActive(GameObject target)
    {
        target.SetActive(!target.activeSelf);
    }

    public void Quit()
    {
        Application.Quit();
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

    public List<string> MajorDecisions = new List<string>();
}

public enum SceneNames
{
    OpenWorld,
    PriestHouse,
    Dhamphir_House,
    Church,
    MainMenu
}
