using System.Collections.Generic;
using System.Linq;
using Dialogue;
using Dialogue.Entry;
using Human;
using Inventory;
using Inventory.Signals;
using LD48;
using Map;
using Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Monads;
using Zenject;

namespace Environment
{
    public class MapCrate : ItemContainer, IInteractable, IClickDialogueTarget
    {
        [SerializeField] private int capacity = 16;
        [ShowInInspector, ReadOnly] private List<Item> items = new();

        [Inject] private MapObjectController mapObjectController;
        [Inject] private SpriteRenderer spriteRenderer;
        [Inject] private VisualsConfig visualsConfig;

        public override int Capacity => capacity;
        public override List<Item> Items => items;

        private void OnEnable()
        {
            ItemsUpdatedEvent.AddListener(UpdateDialogueEntry);
        }

        private void OnDisable()
        {
            ItemsUpdatedEvent.RemoveListener(UpdateDialogueEntry);
        }

        private void Start()
        {
            UpdateDialogueEntry();
        }

        #region IInteractable

        public bool CanBePickedUp => false;

        public IMaybe<Item> MaybeItem => Maybe.Empty<Item>();

        public IMaybe<MapObject> MaybeMapObject => mapObjectController.MapObject.ToMaybe();
        public GameObject GameObject => gameObject;

        public void SetHighlight(bool isLit)
        {
            spriteRenderer.material =
                isLit ? visualsConfig.HighlightedInteractableShader : visualsConfig.RegularInteractableShader;
        }

        public bool CanInteract()
        {
            return true;
        }

        public void Interact(HumanController humanController)
        {
            SignalsHub.DispatchAsync(new ToggleItemContainerCommand(this, GameObject));
            SignalsHub.DispatchAsync(new ShowInventoryCommand());
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }

        #endregion

        #region Dialogue
        
        private void UpdateDialogueEntry()
        {
            var entryDescription = Items.Any()
                ? "Let's see if there is something interesting in that crate..."
                : "Well, that's just an empty crate.";
            DialogueEntry = new SerializedDialogueEntry(entryDescription);
        }
        public IDialogueEntry DialogueEntry { get; private set; } = new SerializedDialogueEntry();
        public void OnPointerClick(PointerEventData eventData)
        {
            SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(DialogueEntry));
        }
        
        #endregion
    }
}