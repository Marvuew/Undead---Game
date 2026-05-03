using UnityEngine;

public class HouseSpawnManager : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
            return;

        SpawnPoint2D[] spawnPoints = FindObjectsByType<SpawnPoint2D>(FindObjectsSortMode.None);

        foreach (SpawnPoint2D spawnPoint in spawnPoints)
        {
            if (spawnPoint.spawnPointId == GameProgressState.ReturnSpawnPointId)
            {
                player.transform.position = spawnPoint.transform.position;
                return;
            }
        }
    }
}