using UnityEngine;

public class PersistentCamera2D : MonoBehaviour
{
    public static PersistentCamera2D Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}