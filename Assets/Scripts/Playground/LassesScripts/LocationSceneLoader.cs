using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocationSceneLoader : MonoBehaviour
{
    [Serializable]
    public class Location
    {
        public string displayName;
        public string sceneName; // MUST match the .unity scene filename (without .unity)
    }

    [SerializeField] private Location[] locations;

    public void LoadLocationByIndex(int index)
    {
        Debug.Log($"[LocationSceneLoader] Button/dropdown triggered. index={index}");

        if (locations == null || locations.Length == 0)
        {
            Debug.LogError("[LocationSceneLoader] No locations set in Inspector.");
            return;
        }

        if (index < 0 || index >= locations.Length)
        {
            Debug.LogError($"[LocationSceneLoader] Index out of range. locations.Length={locations.Length}");
            return;
        }

        string sceneName = locations[index].sceneName;
        Debug.Log($"[LocationSceneLoader] Trying to load scene '{sceneName}'");

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[LocationSceneLoader] sceneName is empty for that location.");
            return;
        }

        // Helpful check: is the scene even in build settings?
        bool found = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) { found = true; break; }
        }

        if (!found)
        {
            Debug.LogError($"[LocationSceneLoader] Scene '{sceneName}' is NOT in Build Settings -> Scenes In Build.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}