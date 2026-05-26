using UnityEngine;

namespace Assets.Scripts.GameScripts
{
    public class MinimapCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private string playerTag = "Player";

        private void LateUpdate()
        {
            if (player == null)
                FindPlayer();

            if (player == null)
                return;

            transform.position = new Vector3(
                player.position.x,
                player.position.y,
                transform.position.z
            );
        }

        private void FindPlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
                player = playerObject.transform;
        }
    }
}