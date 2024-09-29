using System.Collections.Generic;
using System.Linq;
using FunkyCode;
using Human;
using Inventory;
using Inventory.Signals;
using LD48;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utilities.Monads;
using Zenject;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Inject] private HumanController humanController;
        [Inject] private IInventory humanInventory;
        [Inject] private PlayerControls playerInput;

        [SerializeField] private List<Item> initialInventoryItems = new();

        private float horizontal;
        private float vertical;
        
        [SerializeField] protected InputActionReference pointerPositionInput;
        [SerializeField] private LightEventListener lightEventListener;
            
        private PointerEventData clickData;
        private List<RaycastResult> clickResults = new();

        public IInventory Inventory => humanInventory;
        public IMaybe<Item> HandItem => humanInventory.HandItem;
        public HumanState State => humanController.State;
        public double Visibility => lightEventListener.visability;

        private bool isEnabled = true;

        public void OnMove(InputAction.CallbackContext ctx)
        {
            if (!isEnabled)
            {
                horizontal = 0f;
                vertical = 0f;
                return;
            }
            
            OnPlayerMoved();
            
            var moveAmount = ctx.ReadValue<Vector2>(); 
            horizontal = moveAmount.x;
            vertical = moveAmount.y;
        }

        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (!isEnabled) return;
            if (IsPointerOverUIElement())
                return;

            OnPlayerActed();
            humanController.Act();
        }

        public void OnFire2(InputAction.CallbackContext ctx)
        {
            // humanController.ToggleIsAiming();
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!isEnabled) return;

            OnPlayerActed();
            humanController.Interact();
        }

        public void OnInventory(InputAction.CallbackContext ctx)
        {
            if (!isEnabled) return;
            
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
        
        private void OnPlayerMoved()
        {
            SignalsHub.DispatchAsync(new PlayerMovedEvent());
        }
        
        private void OnPlayerActed()
        {
            SignalsHub.DispatchAsync(new PlayerActedEvent());
        }

        private void Awake()
        {
            clickData = new PointerEventData(EventSystem.current);
            
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

        private void FixedUpdate()
        {
            var moveVector = new Vector2(horizontal, vertical);
            humanController.Move(moveVector);
        }
        
        private void OnEnable()
        {
            SignalsHub.AddListener<PlayerHandItemUpdatedEvent>(OnHandItemUpdated);
            SignalsHub.AddListener<PlayerInputEnabledEvent>(OnPlayerInputEnabled);
        }
        
        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayerHandItemUpdatedEvent>(OnHandItemUpdated);
            SignalsHub.RemoveListener<PlayerInputEnabledEvent>(OnPlayerInputEnabled);
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
        
        private void OnPlayerInputEnabled(PlayerInputEnabledEvent evt)
        {
            isEnabled = evt.IsEnabled;
        }
    }
}