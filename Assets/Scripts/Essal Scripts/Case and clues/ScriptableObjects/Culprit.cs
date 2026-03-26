using UnityEngine;

[CreateAssetMenu(menuName = "Case and Clues/New Culprit")]
public class Culprit : ScriptableObject
{
    public string culpritName;
    public string culpritHabitat;
    public string culpritDescription;

    public Sprite homeSprite;
    public Sprite culpritSprite;
}
