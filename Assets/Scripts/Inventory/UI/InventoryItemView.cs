using Signals;
using UI.Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Monads;
using Zenject;

namespace Inventory.UI
{
    public class InventoryItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image itemBackground;
        [SerializeField] private Sprite focusedBackgroundSprite;
        [SerializeField] private Sprite unfocusedBackgroundSprite;

        [Space(10)] [SerializeField] private GameObject itemIcon;
        [SerializeField] private Image itemIconImage;
        
        [Space(10)]
        [SerializeField] private Vector3 tooltipOffset = new(0, 40);

        [Inject] private ItemContainerPanelController interactableContainerPanelController;
        [Inject] private PlayerInventoryPanelController playerInventoryPanelController;

        private IMaybe<Item> _maybeItem = Maybe.Empty<Item>();

        public void SetUp(IMaybe<Item> maybeItem)
        {
            _maybeItem = maybeItem;

            maybeItem.IfPresent(item =>
            {
                itemIcon.SetActive(true);
                itemIconImage.sprite = item.InventoryItemSprite;
            }).IfNotPresent(() => { itemIcon.SetActive(false); });

            HighlightItem(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _maybeItem.IfPresent(_ =>
            {
                if (playerInventoryPanelController.IsVisible && playerInventoryPanelController.CanAddItem())
                {
                    HighlightItem(true);
                }
                else
                {
                    SignalsHub.DispatchAsync(new ShowHoverTooltipCommand(
                        transform.position, 
                        "Inventory is full!",
                        tooltipOffset));
                }
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HighlightItem(false);
            SignalsHub.DispatchAsync(new HideHoverTooltipCommand());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _maybeItem.IfPresent(item =>
            {
                if (playerInventoryPanelController.IsVisible && playerInventoryPanelController.CanAddItem())
                {
                    playerInventoryPanelController.AddItem(item);
                    interactableContainerPanelController.RemoveItem(item);
                }
            });
        }

        private void HighlightItem(bool isHighlighted)
        {
            itemBackground.sprite = isHighlighted ? focusedBackgroundSprite : unfocusedBackgroundSprite;
        }
    }
}