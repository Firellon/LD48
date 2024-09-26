using Dialogue;
using Dialogue.Entry;
using Inventory;
using Inventory.Signals;
using LD48;
using LD48.Cutscenes;
using Map.Actor;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Environment
{
    public class MapExit : MonoBehaviour, IClickDialogueTarget
    {
        [Inject] private SpriteRenderer spriteRenderer;
        [Inject] private VisualsConfig visualsConfig;
        [Inject] private IMapActorRegistry mapActorRegistry;

        private bool playerHasKey;

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
            mapActorRegistry
                .Player
                .IfPresent(player =>
                {
                    UpdateDialogueEntry(player.Inventory);
                })
                .IfNotPresent(() =>
                {
                    Debug.LogWarning("MapExit > Player not found!");
                });
        }

        public void SetHighlight(bool isLit)
        {
            spriteRenderer.material =
                isLit ? visualsConfig.HighlightedInteractableShader : visualsConfig.RegularInteractableShader;
        }

        #region Dialogue

        private void UpdateDialogueEntry(IInventory playerInventory)
        {
            playerHasKey = playerInventory.HasItem(ItemType.Key);
            var entryDescription = playerHasKey
                ? "I have a good feeling about this. Let's come closer and give it a look!"
                : "Hm... that door looks closed.";
            DialogueEntry = new SerializedDialogueEntry(entryDescription);
        }

        public IDialogueEntry DialogueEntry { get; private set; } = new SerializedDialogueEntry();

        public void OnPointerClick(PointerEventData eventData)
        {
            SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(DialogueEntry, () =>
            {
                if (!playerHasKey)
                    return;

                SignalsHub.DispatchAsync(new StartCutsceneSignal
                {
                    Type = CutsceneType.VictoryFoundExit,
                });
            }));
        }

        #endregion
    }
}