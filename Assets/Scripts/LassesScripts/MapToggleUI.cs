using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MapToggleUI : MonoBehaviour
{
    [Header("Drag the whole Map UI panel here (dropdown + buttons etc.)")]
    [SerializeField] private GameObject mapPanel;

    [Header("Key to toggle map")]
    [SerializeField] private Key toggleKey = Key.M;

    [Header("Start hidden")]
    [SerializeField] private bool startHidden = true;

    [Header("Mutual exclusion")]
    [Tooltip("Drag your BookUIController script object here.")]
    [SerializeField] private BookUIController bookController;

    [Header("Scene change behavior")]
    [SerializeField] private bool closeOnSceneChange = true;

    public bool IsMapOpen { get; private set; }

    private void OnEnable()
    {
        if (closeOnSceneChange)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (closeOnSceneChange)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (mapPanel == null)
        {
            Debug.LogError("MapToggleUI: mapPanel is not assigned.");
            return;
        }

        IsMapOpen = !startHidden;
        mapPanel.SetActive(IsMapOpen);
    }

    private void Update()
    {
        if (Keyboard.current == null || mapPanel == null) return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
            ToggleMap();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Whenever we enter a new scene, force-close the map so it isn't in the player's face
        CloseMap();
    }

    public void ToggleMap()
    {
        if (mapPanel == null) return;

        // If we are about to OPEN the map, force-close the book first
        if (!IsMapOpen && bookController != null)
            bookController.ForceCloseBook();

        IsMapOpen = !IsMapOpen;
        mapPanel.SetActive(IsMapOpen);
    }

    public void OpenMap()
    {
        if (mapPanel == null) return;

        if (bookController != null)
            bookController.ForceCloseBook();

        IsMapOpen = true;
        mapPanel.SetActive(true);
    }

    public void CloseMap()
    {
        if (mapPanel == null) return;

        IsMapOpen = false;
        mapPanel.SetActive(false);
    }
}