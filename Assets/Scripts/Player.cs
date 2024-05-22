using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace LD48
{
    public class Player : MonoBehaviour
    {
        public TMP_Text woodAmountText;
        [FormerlySerializedAs("tipMessage")] public TMP_Text tipMessageText;

        private PlayerControls playerInput;
        private Human human;

        private float horizontal;
        private float vertical;

        public void OnMove(InputAction.CallbackContext ctx)
        {
            var moveAmount = ctx.ReadValue<Vector2>();
            horizontal = moveAmount.x;
            vertical = moveAmount.y;
        }
        
        public void OnFire(InputAction.CallbackContext ctx)
        {
            human.Act();
        }
        
        public void OnFire2(InputAction.CallbackContext ctx)
        {
            human.ToggleIsAiming();
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            human.Interact();
        }

        private void Awake()
        {
            playerInput = new PlayerControls();
            playerInput.HumanPlayer.Enable();
            
            playerInput.HumanPlayer.Move.performed += OnMove;
            playerInput.HumanPlayer.Move.canceled += OnMove;
            
            playerInput.HumanPlayer.Fire.performed += OnFire;
            
            playerInput.HumanPlayer.Fire2.performed += OnFire2;
            
            playerInput.HumanPlayer.Interact.performed += OnInteract;
        }

        private void OnDestroy()
        {
            playerInput.HumanPlayer.Disable();
            
            playerInput.HumanPlayer.Move.performed -= OnMove;
            playerInput.HumanPlayer.Move.canceled -= OnMove;
            
            playerInput.HumanPlayer.Fire.performed -= OnFire;
            
            playerInput.HumanPlayer.Fire2.performed -= OnFire2;
            
            playerInput.HumanPlayer.Interact.performed -= OnInteract;
        }

        private void Start()
        {
            human = GetComponent<Human>();
        }

        private void Update()
        {
            // horizontal = Input.GetAxisRaw("Horizontal");
            // vertical = Input.GetAxisRaw("Vertical");

            if (woodAmountText && tipMessageText)
            {
                woodAmountText.text = $"Wood: {human.woodAmount} / {human.maxWoodAmount}";
                tipMessageText.text = human.GetTipMessageText();   
            }
        }

        private void FixedUpdate()
        {
            var moveVector = new Vector2(horizontal, vertical);
            human.Move(moveVector);
        }
    }
}
