using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Dialogue/New Speaker")]
public class DialogueSpeaker : ScriptableObject
{
    public string SpeakerName;

    [Header("Speaker Sprites")]
    public Sprite Happy;
    public Sprite Content;
    public Sprite Sad;
    public Sprite Angry;
}

