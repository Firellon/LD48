using UnityEngine;
using UnityEngine.UI;
using Utilities.Monads;

namespace Inventory.UI
{
    public class InventoryCraftingItemController : MonoBehaviour
    {
        [SerializeField] private GameObject itemIconObject;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Sprite emptyCraftingSprite;

        private IMaybe<Item> currentHandItem = Maybe.Empty<Item>();

        private void Start()
        {
            SetCraftingItem(Maybe.Empty<Item>());
        }

        private void SetCraftingItem(IMaybe<Item> maybeItem)
        {
            currentHandItem = maybeItem;
            currentHandItem.IfPresent(item => { itemIcon.sprite = item.InventoryItemSprite; })
                .IfNotPresent(() => { itemIcon.sprite = emptyCraftingSprite; });
        }
    }
}