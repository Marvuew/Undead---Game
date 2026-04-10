using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public int Humanity;
    public int Undead;

    public void OnEnable()
    {
        GameEvents.AlignmentChange.AddListener(HandleAlignmentChange);
    }

    public void OnDisable()
    {
        GameEvents.AlignmentChange.RemoveListener(HandleAlignmentChange);
    }


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

    public void HandleAlignmentChange(int humanityChange, int undeadChange)
    {
        Humanity += humanityChange;
        Undead += undeadChange;

        Debug.Log($"Humanity {Humanity} + Undead {Undead}");
    }
}
