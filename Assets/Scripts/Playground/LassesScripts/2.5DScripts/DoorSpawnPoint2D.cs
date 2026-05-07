using UnityEngine;

public class DoorSpawnPoint2D : MonoBehaviour
{
    [Header("Spawn Point ID")]
    public SpawnPointID spawnPointID = SpawnPointID.None;

    // Compatibility for older scripts that still use spawnPoint.sceneName
    public string sceneName => spawnPointID.ToString();
}