using UnityEngine;
[CreateAssetMenu(menuName = "Dialogue/New Callback")]
public class Callback : ScriptableObject
{
    // For Context
    [Tooltip("Person that the callback comes from")]
    public DialogueSpeaker CallbackPerson;
    public string CallbackContext;
}
