using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Dialogue/New Speaker")]
public class DialogueSpeaker : ScriptableObject
{
    public string SpeakerName;
    public Sprite SpeakerSprite;
}

