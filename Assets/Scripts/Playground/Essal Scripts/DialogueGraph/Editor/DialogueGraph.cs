using UnityEngine;
using UnityEditor;
using Unity.GraphToolkit.Editor;
using JetBrains.Annotations;
using System;

[Serializable]
[Graph(AssetExtension)]
public class DialogueGraph : Graph
{
    public const string AssetExtension = "dialoguegraph";

    [MenuItem("Assets/Create/Dialogue Graph", false)]
    private static void CreateAssetFile()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DialogueGraph> ();
    }
        
}
