using Dialogue;
using Dialogue.Entry;
using Human;
using Inventory;
using LD48;
using Map;
using Player;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Monads;
using Zenject;

namespace Environment
{
    public class MapJournalEntry : MonoBehaviour, IInteractable, IClickDialogueTarget
    {
        [Inject] private VisualsConfig visualsConfig;
        [Inject] private MapObjectController mapObjectController;
        [Inject] private SpriteRenderer spriteRenderer;

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
            var playerController = humanController.GetComponent<PlayerController>();
            if (playerController != null)
            {
                SignalsHub.DispatchAsync(new MapDiaryCollectedSignal(this)); 
                Remove();
            }
            else
            {
                Debug.LogError("Non-Player is trying to Interact with the Diary!");
            }
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }
        
        #region Dialogue

        public void OnPointerClick(PointerEventData eventData)
        {
            SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(DialogueEntry));
        }

        public IDialogueEntry DialogueEntry { get; } = new SerializedDialogueEntry
        {
            EntryDescription =
                "Someone left a note lying on a ground. I better pick it up before the rains wash it away."
        };

        #endregion
    }
}