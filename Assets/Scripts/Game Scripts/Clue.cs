using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(menuName = "Case and Clues/New Clue")]
public class Clue : ScriptableObject
{
    [TextArea(3, 5)] public string initialDescription;
    public List<UndeadType> undeadTypes;
}
