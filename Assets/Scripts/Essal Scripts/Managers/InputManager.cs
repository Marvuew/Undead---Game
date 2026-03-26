using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public UnityEvent OnNecroLexiconActivated = new UnityEvent();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E");
            OnNecroLexiconActivated.Invoke();
        }
    }
}
