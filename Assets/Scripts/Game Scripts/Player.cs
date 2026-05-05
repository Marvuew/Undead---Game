using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

namespace Assets.Scripts.GameScripts
{
    public class Player : MonoBehaviour
    {
        public int humanity = 50;
        public int undead = 50;

        List<string> inventory;
        public RuntimeInteractable currentInteractable;

        private Vector2 moveInput;
        public bool interacting;
        [SerializeField] float speed;
        [SerializeField] SpriteRenderer sprite;
        private Rigidbody2D rb;

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
        void FixedUpdate()
        {
            if (interacting) return;
            rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
            if (moveInput.x != 0)
                sprite.flipX = moveInput.x < 0;
        }
        public void ChangeHumanity(int change) { humanity += change; }
        public void ChangeUndead(int change) { undead += change; }

        public void OnMove(InputAction.CallbackContext input) 
        {
            moveInput = input.ReadValue<Vector2>(); 
        }
        public void OnInteract(InputAction.CallbackContext input) 
        { 
            if(input.performed && currentInteractable != null) 
            { 
                currentInteractable.startInteraction();
                interacting = true;
            }
        }
    }
}
