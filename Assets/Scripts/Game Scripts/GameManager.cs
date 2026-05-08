using Assets.Scripts.GameScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [SerializeField] private GameObject pauseMenu;

    public IntroSequence gameIntroSequence;

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

    }

    public void StartGame()
    {
        StartCoroutine(gameIntroSequence.HandleIntroDialogue());
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
}

public enum SceneNames
{
    OpenWorld,
    PriestHouse,
    Dhamphir_House,
    Church,
    MainMenu
}
