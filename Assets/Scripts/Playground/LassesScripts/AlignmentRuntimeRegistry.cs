using System;
using UnityEngine;

public static class AlignmentRuntimeRegistry
{
    public static AlignmentSlider Slider { get; private set; }
    public static AlignmentPointPopupSpawner PopupSpawner { get; private set; }
    public static MessageChainDialogue Dialogue { get; private set; }

    public static event Action OnChanged;

    public static void Register(AlignmentSlider slider)
    {
        Slider = slider;
        OnChanged?.Invoke();
        Debug.Log("[Registry] Registered AlignmentSlider: " + slider.name);
    }

    public static void Register(AlignmentPointPopupSpawner spawner)
    {
        PopupSpawner = spawner;
        OnChanged?.Invoke();
        Debug.Log("[Registry] Registered PopupSpawner: " + spawner.name);
    }

    public static void Register(MessageChainDialogue dialogue)
    {
        Dialogue = dialogue;
        OnChanged?.Invoke();
        Debug.Log("[Registry] Registered MessageChainDialogue: " + dialogue.name);
    }

    public static void Unregister(AlignmentSlider slider)
    {
        if (Slider == slider) Slider = null;
        OnChanged?.Invoke();
    }

    public static void Unregister(AlignmentPointPopupSpawner spawner)
    {
        if (PopupSpawner == spawner) PopupSpawner = null;
        OnChanged?.Invoke();
    }

    public static void Unregister(MessageChainDialogue dialogue)
    {
        if (Dialogue == dialogue) Dialogue = null;
        OnChanged?.Invoke();
    }
}
