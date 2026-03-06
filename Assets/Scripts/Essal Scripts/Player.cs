using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour
{
    public static Player Instance;

    int humanity;
    int stamina;

    List<string> inventory;

    public static event Action<float> OnHumanityChanged;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(this);
    }

    public void ChangeHumanity(float value)
    {
        OnHumanityChanged.Invoke(value);
    }


}
