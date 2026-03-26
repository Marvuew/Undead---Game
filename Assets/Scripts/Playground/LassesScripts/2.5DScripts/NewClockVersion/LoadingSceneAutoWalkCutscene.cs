using UnityEngine;

public class LoadingSceneAutoWalkCutscene : MonoBehaviour
{
    public static LoadingSceneAutoWalkCutscene Instance;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool autoFindPlayer = true;

    [Header("Auto Walk")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private Vector2 moveDirection = Vector2.right;
    [SerializeField] private bool useFixedUpdate = true;

    [Header("Fade In")]
    [SerializeField] private bool fadeFromBlackOnStart = true;
    [SerializeField] private float fadeFromBlackSeconds = 1f;

    private Transform playerTransform;
    private Rigidbody2D playerRb;
    private PlayerMovement2D playerMovement;
    private bool cutsceneActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (fadeFromBlackOnStart && PersistentScreenFader.Instance != null)
            PersistentScreenFader.Instance.FadeFromBlack(fadeFromBlackSeconds);

        if (autoFindPlayer)
            FindPlayer();

        BeginCutscene();
    }

    private void Update()
    {
        if (!useFixedUpdate)
            MovePlayer(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
            MovePlayer(Time.fixedDeltaTime);
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning("LoadingSceneAutoWalkCutscene: No player with tag '" + playerTag + "' found.");
            return;
        }

        playerTransform = player.transform;
        playerRb = player.GetComponent<Rigidbody2D>();
        playerMovement = player.GetComponent<PlayerMovement2D>();
    }

    public void BeginCutscene()
    {
        if (playerTransform == null)
            FindPlayer();

        if (playerTransform == null)
            return;

        if (playerMovement != null)
            playerMovement.enabled = false;

        cutsceneActive = true;
    }

    public void EndCutscene()
    {
        cutsceneActive = false;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private void MovePlayer(float deltaTime)
    {
        if (!cutsceneActive || playerTransform == null)
            return;

        Vector2 dir = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector2.right;
        Vector3 delta = new Vector3(dir.x, dir.y, 0f) * walkSpeed * deltaTime;

        if (playerRb != null)
        {
            playerRb.MovePosition(playerRb.position + new Vector2(delta.x, delta.y));
        }
        else
        {
            playerTransform.position += delta;
        }
    }
}