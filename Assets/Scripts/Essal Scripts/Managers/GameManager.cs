using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public RuntimeClue[] runtimeClues;

    private void Awake()
    {
        instance = this;
    }
}
