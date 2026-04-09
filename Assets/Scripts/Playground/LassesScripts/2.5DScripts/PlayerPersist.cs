using UnityEngine;

public class PersistentPlayer2D : MonoBehaviour
{
    public static PersistentPlayer2D Instance;

    void Awake()
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
