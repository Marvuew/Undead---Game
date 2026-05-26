using UnityEngine;

[CreateAssetMenu(menuName = "Case and Clues/New Undead")]
public class Undead : ScriptableObject
{
    public UndeadType undeadType;
    public string description;
    public InteractableScriptableObject undeadInteractable;
    public Sprite undeadSprite;
    public Sprite cardSprite;
}

public enum UndeadType { Strigoi, Lamia, Draugr, Banshee, Myling, Nisse, Vaettir, Changeling, Fairy, WillOWisp }
