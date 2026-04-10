using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject player;
    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        Vector3 targetPos = new Vector3(player.transform.position.x,player.transform.position.y+2, transform.position.z);
        targetPos.z = transform.position.z;

        transform.position = Vector3.SmoothDamp( transform.position, targetPos, ref velocity, 0.2f );
    }
}
