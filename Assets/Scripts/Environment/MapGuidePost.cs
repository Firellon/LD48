using Dialogue;
using Dialogue.Entry;
using Human;
using Inventory;
using LD48;
using Map;
using Signals;
using Stranger;
using UI;
using UI.Signals;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Monads;
using Zenject;

namespace Environment
{
    public class MapGuidePost : MonoBehaviour, IInteractable, IHoverTooltipTarget, IClickDialogueTarget, IPointerClickHandler
    {
        [Inject] private VisualsConfig visualsConfig;
        [Inject] private MapObjectController mapObjectController;
        [Inject] private SpriteRenderer spriteRenderer;
        [Inject] private ICharacterRegistry characterRegistry;
        
        [SerializeField] private Vector2 leftBottomTooltipOffset;
        [SerializeField] private Vector2 rightTopTooltipOffset;

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
            characterRegistry.PlayerCharacter.IfPresent(playerCharacter =>
            {
                DialogueEntry = new SerializedDialogueEntry
                {
                    EntryCharacter = playerCharacter,
                    EntryTitle = playerCharacter.CharacterName,
                    EntryDescription =  GuidePostText != string.Empty 
                        ? $"The text on this post says: \"{GuidePostText}\"."
                        : "This post is empty."
                };
            }).IfNotPresent(() =>
            {
                Debug.LogError("Player Character not found in the CharacterRegistry!");
            });
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

        public string TooltipText => GuidePostText;
        public Vector2 LeftBottomTooltipOffset => leftBottomTooltipOffset;
        public Vector2 RightTopTooltipOffset => rightTopTooltipOffset;

        public IDialogueEntry DialogueEntry { get; private set; } = new SerializedDialogueEntry {};
        public void OnPointerClick(PointerEventData eventData)
        {
            SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(DialogueEntry));
        }
    }
}