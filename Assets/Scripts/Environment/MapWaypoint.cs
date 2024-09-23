using Dialogue;
using Dialogue.Entry;
using Human;
using Inventory;
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
    public class MapWaypoint : MonoBehaviour, IInteractable, IClickDialogueTarget
    {
        [Inject] private SpriteRenderer spriteRenderer;
        [Inject] private VisualsConfig visualsConfig;
        [Inject] private MapObjectController mapObjectController;

        [ShowInInspector, ReadOnly] private bool isActivated;

        private void Start()
        {
            UpdateDialogueEntry();
        }

        public void SetHighlight(bool isLit)
        {
            spriteRenderer.material =
                isLit ? visualsConfig.HighlightedInteractableShader : visualsConfig.RegularInteractableShader;
        }

        #region Dialogue
        
        private void UpdateDialogueEntry()
        {
            DialogueEntry = new SerializedDialogueEntry
            {
                EntryDescription = isActivated 
                    ? "This one does not seem particularly useful anymore." 
                    : "Looks interesting. I should probably check it out."
            };
        }
        
        public IDialogueEntry DialogueEntry { get; private set; } = new SerializedDialogueEntry();

        public void OnPointerClick(PointerEventData eventData)
        {
            SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(DialogueEntry));
        }
        
        #endregion

        #region IInteractable

        public bool CanBePickedUp => false;
        public IMaybe<Item> MaybeItem => Maybe.Empty<Item>();
        public IMaybe<MapObject> MaybeMapObject => mapObjectController.MapObject.ToMaybe();
        public GameObject GameObject => gameObject;
        public bool CanInteract()
        {
            return !isActivated;
        }

        public void Interact(HumanController humanController)
        {
            isActivated = true;
            UpdateDialogueEntry();
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }

        #endregion
    }
}