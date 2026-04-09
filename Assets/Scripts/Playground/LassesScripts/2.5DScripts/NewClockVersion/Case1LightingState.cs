using UnityEngine;
using UnityEngine.SceneManagement;

public class Case1DayState : MonoBehaviour
{
    public static Case1DayState Instance;

    [Range(0f, 1f)]
    [SerializeField] private float normalizedDayProgress = 0f;

    [SerializeField] private string[] case1SceneNames;

    public float NormalizedDayProgress
    {
        get => normalizedDayProgress;
        set => normalizedDayProgress = Mathf.Clamp01(value);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
    }

    public bool IsCurrentScenePartOfCase1()
    {
        if (case1SceneNames == null || case1SceneNames.Length == 0)
            return true;

        string currentScene = SceneManager.GetActiveScene().name;

        for (int i = 0; i < case1SceneNames.Length; i++)
        {
            if (case1SceneNames[i] == currentScene)
                return true;
        }

        return false;
    }

    public void ResetCase1Day()
    {
        normalizedDayProgress = 0f;
    }
}