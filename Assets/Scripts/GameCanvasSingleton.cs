using UnityEngine;

public class GameCanvasSingleton : MonoBehaviour
{
    public static GameCanvasSingleton instance;

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
}
