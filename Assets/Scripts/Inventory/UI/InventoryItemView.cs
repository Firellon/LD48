using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Monads;

namespace Inventory.UI
{
    public class InventoryItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image itemBackground;
        [SerializeField] private Sprite focusedBackgroundSprite;
        [SerializeField] private Sprite unfocusedBackgroundSprite;

        [Space(10)] [SerializeField] private GameObject itemIcon;
        [SerializeField] private Image itemIconImage;
        
        private IMaybe<Item> _maybeItem = Maybe.Empty<Item>();

        public void SetUp(IMaybe<Item> maybeItem)
        {
            _maybeItem = maybeItem;
            
            maybeItem.IfPresent(item =>
            {
                itemIcon.SetActive(true);
                itemIconImage.sprite = item.InventoryItemSprite;
            }).IfNotPresent(() =>
            {
                itemIcon.SetActive(false);
            });
            
            HighlightItem(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _maybeItem.IfPresent(_ =>
            {
                HighlightItem(true);
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HighlightItem(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _maybeItem.IfPresent(item =>
            {
                // TODO: Move into Player's Inventory
            });
        }
        
        private void HighlightItem(bool isHighlighted)
        {
            itemBackground.sprite = isHighlighted ? focusedBackgroundSprite : unfocusedBackgroundSprite;
        }
    }
}