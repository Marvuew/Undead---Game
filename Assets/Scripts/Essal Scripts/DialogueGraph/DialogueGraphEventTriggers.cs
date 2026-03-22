using UnityEngine;

public class DialogueGraphEventTriggers : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public static void Trigger(string actionName)
    {
        switch (actionName)
        {
            case "Kill":
                Kill();
                break;
            case "Resolve":
                Resolve();
                break;
            default:
                Debug.Log("Couldnt find a action to execute");
                break;

        }
    }

    public static void Kill()
    {
        Debug.Log("Kill");
    }

    public static void Resolve()
    {
        Debug.Log("Resolve");
    }
}
