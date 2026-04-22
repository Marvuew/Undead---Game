using UnityEngine;

[CreateAssetMenu(fileName = "NewCreature", menuName = "Creatures/Creature")]
public class CreatureData : ScriptableObject
{
    public string creatureName;
    public Sprite image;
    [TextArea] public string description;


}
