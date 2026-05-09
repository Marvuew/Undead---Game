using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.GameScripts
{
    public class Player : MonoBehaviour
    {
        public int humanity = 50;
        public int undead = 50;
        [SerializeField] Animator animator;

        private List<string> inventory;
        public RuntimeInteractable currentInteractable;

        [SerializeField] Vector2 moveInput;
        public bool interacting;

        [SerializeField] private float speed;
        [SerializeField] private SpriteRenderer sprite;

        private Rigidbody2D rb;

        private Vector3 lastPosition;
        private bool internalMovement;

        public static Player Instance { get; private set; }

        private Player() { }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            lastPosition = transform.position;
        }

        private void FixedUpdate()
        {
            internalMovement = true;

            if (!interacting && rb != null)
            {
                rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
            }

            internalMovement = false;

            if (sprite != null && moveInput.x != 0)
                sprite.flipX = moveInput.x < 0;
            AnimatePlayer(moveInput);
        }

        public void ChangeHumanity(int change)
        {
            humanity += change;
        }

        public void ChangeUndead(int change)
        {
            undead += change;
        }

        public void OnMove(InputAction.CallbackContext input)
        {
            moveInput = input.ReadValue<Vector2>();
        }

        public void OnInteract(InputAction.CallbackContext input)
        {
            if (input.performed && currentInteractable != null)
            {
                currentInteractable.startInteraction();
                interacting = true;
            }
        }

        public void SetPosition(Vector2 position)
        {
            internalMovement = true;

            transform.position = position;

            if (rb != null)
            {
                rb.position = position;
                rb.linearVelocity = Vector2.zero;
            }

            internalMovement = false;

            lastPosition = transform.position;
        }

        public void StopMovement()
        {
            moveInput = Vector2.zero;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        public void MovePlayerToSpawnPoint()
        {
            if (TransitionState2D.HasTransition)
                return;

            GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

            if (spawnPoint == null)
                return;

            SetPosition(spawnPoint.transform.position);
        }
        private void AnimatePlayer(Vector2 movement)
        {
            animator.SetFloat("x",movement.x);
            animator.SetFloat("y", movement.y);
            animator.SetBool("isWalking", (movement != Vector2.zero));
        }
    }
}