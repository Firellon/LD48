using UnityEngine;
using UnityEngine.UI;
using Utilities.Monads;

namespace Inventory.UI
{
    public class InventoryItemView : MonoBehaviour
    {
        [SerializeField] private GameObject itemIcon;
        [SerializeField] private Image itemIconImage;

        public void SetUp(IMaybe<Item> maybeItem)
        {
            maybeItem.IfPresent(item =>
            {
                itemIcon.SetActive(true);
                itemIconImage.sprite = item.InventoryItemSprite;
            }).IfNotPresent(() => { itemIcon.SetActive(false); });
        }
    }
}