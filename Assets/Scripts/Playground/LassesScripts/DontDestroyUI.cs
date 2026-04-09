using UnityEngine;

public class DontDestroyUI : MonoBehaviour
{
    private static DontDestroyUI instance;

    private void Awake()
    {
        // If another persistent UI already exists, destroy this duplicate
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Keep this GameObject (and all its children) when changing scenes
        DontDestroyOnLoad(gameObject);
    }
}
