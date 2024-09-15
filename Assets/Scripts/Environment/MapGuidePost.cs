using Dialogue;
using Dialogue.Entry;
using Human;
using Inventory;
using LD48;
using Map;
using Signals;
using UI.Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Monads;
using Zenject;

namespace Environment
{
    public class MapGuidePost : MonoBehaviour, IInteractable, IClickDialogueTarget, IPointerClickHandler
    {
        [Inject] private VisualsConfig visualsConfig;
        [Inject] private MapObjectController mapObjectController;
        [Inject] private SpriteRenderer spriteRenderer;

        public bool CanBePickedUp => false;
        public IMaybe<Item> MaybeItem => Maybe.Empty<Item>();
        public IMaybe<MapObject> MaybeMapObject => mapObjectController.MapObject.ToMaybe();
        public GameObject GameObject => gameObject;

        private string guidePostText = string.Empty;
        public string GuidePostText
        {
            get => guidePostText;
            set
            {
                guidePostText = value;
                UpdateDialogueEntry();
            }
        }

        public void Start()
        {
            UpdateDialogueEntry();
        }

        private void UpdateDialogueEntry()
        {
            DialogueEntry = new SerializedDialogueEntry
            {
                EntryDescription =  GuidePostText != string.Empty 
                    ? $"This post says: \"{GuidePostText}\". I wonder what's the meaning of this..."
                    : "This post is empty."
            };
        }

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
            SignalsHub.DispatchAsync(new ShowTextInputPopupCommand(
                GuidePostText,
                "Guidepost sign:",
                newGuidePostText => GuidePostText = newGuidePostText.Trim()));
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }

        public IDialogueEntry DialogueEntry { get; private set; } = new SerializedDialogueEntry();
        public void OnPointerClick(PointerEventData eventData)
        {
            SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(DialogueEntry));
        }
    }
}