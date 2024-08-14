using Human;
using Inventory;
using LD48;
using Map;
using Signals;
using UI;
using UI.Signals;
using UnityEngine;
using Utilities.Monads;
using Zenject;

namespace Environment
{
    public class MapGuidePost : MonoBehaviour, IInteractable, IHoverTooltipTarget
    {
        [Inject] private VisualsConfig visualsConfig;
        [Inject] private MapObjectController mapObjectController;
        [Inject] private SpriteRenderer spriteRenderer;
        
        [SerializeField] private Vector2 leftBottomTooltipOffset;
        [SerializeField] private Vector2 rightTopTooltipOffset;

        public bool CanBePickedUp => false;
        public IMaybe<Item> MaybeItem => Maybe.Empty<Item>();
        public IMaybe<MapObject> MaybeMapObject => mapObjectController.MapObject.ToMaybe();
        public GameObject GameObject => gameObject;
        public string GuidePostText { get; set; } = string.Empty;

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
                newGuidePostText => GuidePostText = newGuidePostText));
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }

        public string TooltipText => GuidePostText;
        public Vector2 LeftBottomTooltipOffset => leftBottomTooltipOffset;
        public Vector2 RightTopTooltipOffset => rightTopTooltipOffset;
    }
}