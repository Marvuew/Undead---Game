using UnityEngine;

[CreateAssetMenu(menuName = "Scene/Fade Settings")]
public class SceneFadeSettings : ScriptableObject
{
    public float fadeDuration = 1f;
    public float stayBlackDuration = 1f;
    public Color fadeColor = Color.black;
}