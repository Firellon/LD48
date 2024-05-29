using System;
using Human;
using Inventory;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;

namespace LD48
{
    public class Player : MonoBehaviour
    {
        public TMP_Text woodAmountText;
        [FormerlySerializedAs("tipMessage")] public TMP_Text tipMessageText;

        [Inject] private HumanController humanController;
        [Inject] private IItemContainer humanInventory;

        private PlayerControls playerInput;
        private float horizontal;
        private float vertical;

        public IItemContainer Inventory => humanInventory;

        public void OnMove(InputAction.CallbackContext ctx)
        {
            var moveAmount = ctx.ReadValue<Vector2>();
            horizontal = moveAmount.x;
            vertical = moveAmount.y;
        }

        public void OnFire(InputAction.CallbackContext ctx)
        {
            humanController.Act();
        }

        public void OnFire2(InputAction.CallbackContext ctx)
        {
            humanController.ToggleIsAiming();
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            humanController.Interact();
        }

        public void OnInventory(InputAction.CallbackContext ctx)
        {
            SignalsHub.DispatchAsync(new ToggleInventoryCommand());
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

            playerInput.HumanPlayer.Inventory.performed += OnInventory;
        }

        private void OnDestroy()
        {
            playerInput.HumanPlayer.Disable();

            playerInput.HumanPlayer.Move.performed -= OnMove;
            playerInput.HumanPlayer.Move.canceled -= OnMove;

            playerInput.HumanPlayer.Fire.performed -= OnFire;

            playerInput.HumanPlayer.Fire2.performed -= OnFire2;

            playerInput.HumanPlayer.Interact.performed -= OnInteract;

            playerInput.HumanPlayer.Inventory.performed -= OnInventory;
        }

        private void Update()
        {
            // horizontal = Input.GetAxisRaw("Horizontal");
            // vertical = Input.GetAxisRaw("Vertical");

            if (woodAmountText && tipMessageText)
            {
                woodAmountText.text =
                    $"Wood: {humanController.Inventory.GetItemAmount(ItemType.Wood)} / {humanController.Inventory.ItemSlotCount}";
                tipMessageText.text = humanController.GetTipMessageText();
            }
        }

        private void FixedUpdate()
        {
            var moveVector = new Vector2(horizontal, vertical);
            humanController.Move(moveVector);
        }
    }
}