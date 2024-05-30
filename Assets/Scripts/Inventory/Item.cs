using LD48;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(menuName = "Create Item SO", fileName = "New Item", order = 0)]
    public class Item : ScriptableObject
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private Sprite itemSprite;
        [SerializeField] private Sprite inventoryItemSprite;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private bool canBePickedUp;
        [SerializeField] private bool canBeDropped;

        public ItemType ItemType => itemType;
        public Sprite ItemSprite => itemSprite;
        public Sprite InventoryItemSprite => inventoryItemSprite;
        public GameObject ItemPrefab => itemPrefab;
        public bool CanBePickedUp => canBePickedUp;
        public bool CanBeDropped => canBeDropped;

        public bool Equals(Item item)
        {
            return item.ItemType == itemType;
        }
    }
}