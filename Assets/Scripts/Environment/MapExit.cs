using Dialogue;
using Dialogue.Entry;
using Inventory;
using Inventory.Signals;
using LD48;
using Map.Actor;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Environment
{
    public class MapExit : MonoBehaviour, IClickDialogueTarget, IPointerClickHandler
    {
        [Inject] private IMapActorRegistry mapActorRegistry; 
        private void OnEnable()
        {
            SignalsHub.AddListener<PlayerInventoryUpdatedEvent>(OnPlayerInventoryUpdated);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayerInventoryUpdatedEvent>(OnPlayerInventoryUpdated);
        }

        private void OnPlayerInventoryUpdated(PlayerInventoryUpdatedEvent evt)
        {
            UpdateDialogueEntry(evt.PlayerInventory);
        }

        private void Start()
        {
            mapActorRegistry.Player.IfPresent(player =>
            {
                UpdateDialogueEntry(player.Inventory);
            });
            
        }

        #region Dialogue
        
        private void UpdateDialogueEntry(IInventory playerInventory)
        {
            DialogueEntry = new SerializedDialogueEntry
            {
                EntryDescription = playerInventory.HasItem(ItemType.Key)
                    ? "I have a good feeling about this. Let's come closer and give it a look!"
                    : "Hm... that door looks closed."
            };
        }
        public IDialogueEntry DialogueEntry { get; private set; } = new SerializedDialogueEntry();
        public void OnPointerClick(PointerEventData eventData)
        {
            SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(DialogueEntry));
        }
        
        #endregion
    }
}