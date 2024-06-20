using Player;
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
        private PlayerController _player;

        public void SetUp(IMaybe<Item> maybeItem, PlayerController player)
        {
            _maybeItem = maybeItem;
            _player = player;

            maybeItem.IfPresent(item =>
            {
                itemIcon.SetActive(true);
                itemIconImage.sprite = item.InventoryItemSprite;
            }).IfNotPresent(() => { itemIcon.SetActive(false); });

            HighlightItem(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _maybeItem.IfPresent(item =>
            {
                if (item.IsHandItem)
                {
                    HighlightItem(true);
                }
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HighlightItem(false);
        }

        private void HighlightItem(bool isHighlighted)
        {
            itemBackground.sprite = isHighlighted ? focusedBackgroundSprite : unfocusedBackgroundSprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _maybeItem.IfPresent(item =>
            {
                if (item.IsHandItem)
                {
                    _player.HandItem.IfPresent((handItem) =>
                    {
                        _player.Inventory.RemoveItem(item);
                        _player.Inventory.AddItem(handItem);
                        _player.Inventory.SetHandItem(item.ToMaybe());
                    }).IfNotPresent(() =>
                    {
                        _player.Inventory.RemoveItem(item);
                        _player.Inventory.SetHandItem(item.ToMaybe());
                    });
                }
            });
        }
    }
}