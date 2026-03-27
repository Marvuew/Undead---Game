using UnityEngine;

[CreateAssetMenu(menuName = "Case and Clues/New Clue")]
public class Clue : ScriptableObject
{
    public string clueName;
    public Sprite clueSprite;

    [TextArea(3,10)]
    public string clueDescription;
}
