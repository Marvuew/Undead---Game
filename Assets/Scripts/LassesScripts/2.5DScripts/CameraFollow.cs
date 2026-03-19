using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.12f;

    private Vector3 velocity = Vector3.zero;
    private float fixedZ;

    private void Awake()
    {
        fixedZ = transform.position.z;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ReacquireTarget();
        SnapToTarget();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReacquireTarget();
        SnapToTarget();
    }

    private void ReacquireTarget()
    {
        if (PersistentPlayer2D.Instance != null)
        {
            target = PersistentPlayer2D.Instance.transform;
            return;
        }

        PlayerMovement2D player = FindObjectOfType<PlayerMovement2D>();
        if (player != null)
            target = player.transform;
    }

    public void SnapToTarget()
    {
        if (target == null)
            return;

        transform.position = new Vector3(target.position.x, target.position.y, fixedZ);
        velocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            ReacquireTarget();
            return;
        }

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, fixedZ);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );
    }
}