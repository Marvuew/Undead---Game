using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

namespace Assets.Scripts.GameScripts
{
    class Player : MonoBehaviour
    {
        public int humanity = 50;
        public int undead = 50;

        List<string> inventory;
        public interactable currentInteractable;

        private Vector2 moveInput;
        public bool interacting;
        [SerializeField] float speed;
        [SerializeField] SpriteRenderer sprite;


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
        }
        private void Update()
        {
            if (interacting) return;
            transform.position += (Vector3) moveInput * speed * Time.deltaTime;
            if(moveInput.x != 0)
                sprite.flipX = (moveInput.x > 0) ? false : true;
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
