using UnityEngine;

[CreateAssetMenu(menuName = "Case and Clues/New Culprit")]
public class Suspect : ScriptableObject
{
    public Undead type;
    public string habitat;
    public string description;

    public Sprite homeSprite;
    public Sprite sprite;
}
