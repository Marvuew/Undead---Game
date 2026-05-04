using Assets.Scripts.GameScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [SerializeField] private GameObject pauseMenu;

    // SCENES STORAGE IDEA
    //public static readonly string MAIN_MENU = "Game";
    //public static readonly string WORLD = "World";
    //public static readonly string PRIEST_HOUSE = "Priest House";

    IntroSequence gameIntroSequence;
    bool firstPlayThrough = true;

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

    private void Start()
    {
        gameIntroSequence = FindAnyObjectByType<IntroSequence>();
        gameIntroSequence.StartPanelAnimation();
    }

    public void StartGame()
    {
        if (firstPlayThrough)
        {
            StartCoroutine(gameIntroSequence.StartIntroDialogue());
            firstPlayThrough = false;
        }
        else
        {
            SceneManager.LoadScene(SceneNames.Dhamphir_House.ToString());
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneNames.Priest_House.ToString());
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
}

public enum SceneNames
{
    Day1,
    Priest_House,
    Dhamphir_House,
    Day4
}
