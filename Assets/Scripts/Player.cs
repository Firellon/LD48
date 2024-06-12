using System;
using System.Collections.Generic;
using System.Linq;
using Human;
using Inventory;
using Inventory.Signals;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utilities.Monads;
using Zenject;

namespace LD48
{
    public class Player : MonoBehaviour
    {
        [FormerlySerializedAs("tipMessage")] public TMP_Text tipMessageText;

        [Inject] private HumanController humanController;
        [Inject] private IItemContainer humanInventory;

        [SerializeField] private List<Item> initialInventoryItems = new();

        private PlayerControls playerInput;
        private float horizontal;
        private float vertical;
        
        [SerializeField] protected InputActionReference pointerPositionInput;
            
        private PointerEventData clickData;
        private List<RaycastResult> clickResults = new();

        public IItemContainer Inventory => humanInventory;
        public IMaybe<Item> HandItem => humanInventory.HandItem;
        

        public void OnMove(InputAction.CallbackContext ctx)
        {
            var moveAmount = ctx.ReadValue<Vector2>(); 
            horizontal = moveAmount.x;
            vertical = moveAmount.y;
        }

        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (IsPointerOverUIElement())
                return;
            
            humanController.Act();
        }

        public void OnFire2(InputAction.CallbackContext ctx)
        {
            // humanController.ToggleIsAiming();
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            humanController.Interact();
        }

        public void OnInventory(InputAction.CallbackContext ctx)
        {
            SignalsHub.DispatchAsync(new ToggleInventoryCommand());
        }
        
        private bool IsPointerOverUIElement()
        {
            var mousePosition = Mouse.current.position;
            clickData.position = new Vector2(mousePosition.x.value, mousePosition.y.value); // pointerPositionInput.action.ReadValue<Vector2>();
            clickResults.Clear();

            EventSystem.current.RaycastAll(clickData, clickResults);

            var isPointerOverUIElement = clickResults.Count > 0 &&
                                         clickResults.Any(it => it.gameObject.layer == LayerMask.NameToLayer("UI"));
            return isPointerOverUIElement;
        }

        private void Awake()
        {
            clickData = new PointerEventData(EventSystem.current);
            
            playerInput = new PlayerControls();
            playerInput.HumanPlayer.Enable();

            playerInput.HumanPlayer.Move.performed += OnMove;
            playerInput.HumanPlayer.Move.canceled += OnMove;

            playerInput.HumanPlayer.Fire.performed += OnFire;

            playerInput.HumanPlayer.Fire2.performed += OnFire2;

            playerInput.HumanPlayer.Interact.performed += OnInteract;

            playerInput.HumanPlayer.Inventory.performed += OnInventory;
        }

        private void Start()
        {
            foreach (var item in initialInventoryItems)
            {
                humanInventory.AddItem(item);
            }
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

            if (tipMessageText)
            {
                tipMessageText.text = humanController.GetTipMessageText();
            }
        }

        private void FixedUpdate()
        {
            var moveVector = new Vector2(horizontal, vertical);
            humanController.Move(moveVector);
        }
        
        private void OnEnable()
        {
            SignalsHub.AddListener<PlayerHandItemUpdatedEvent>(OnHandItemUpdated);
        }
        
        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayerHandItemUpdatedEvent>(OnHandItemUpdated);
        }

        private void OnHandItemUpdated(PlayerHandItemUpdatedEvent evt)
        {
            evt.MaybeItem.IfPresent(item =>
            {
                humanController.SetIsAiming(item.ItemType == ItemType.Pistol); // TODO: Check for firearms instead?
            }).IfNotPresent(() =>
            {
                humanController.SetIsAiming(false);
            });
        }
    }
}