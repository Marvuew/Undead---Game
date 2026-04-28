using UnityEngine;

[CreateAssetMenu(menuName = "Create new Interactable")]
public class InteractableScriptableObject : ScriptableObject
{
    [Header("Set these variables ONLY!")]
    public Sprite interactableSprite;
    public InteractableType interactableType;
    public RuntimeDialogueGraph dialogue;

    [Header("Set By Baker Script - DONT TOUCH")]
    public Vector3 position;
    public SceneNames homeScene;
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;

    public void RecordPosition(Vector3 newPos)
    {
        position = newPos;
    #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
    #endif
    }
}
