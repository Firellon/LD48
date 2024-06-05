using System.Collections.Generic;
using System.Linq;
using LD48;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("isUsable")] [FormerlySerializedAs("canUse")] [SerializeField] private bool isHandItem;
        [SerializeField] private bool isCraftable;
        [SerializeField] private ItemTypeToIntSerializedDictionary craftingRequirements = new ();

        public ItemType ItemType => itemType;
        public Sprite ItemSprite => itemSprite;
        public Sprite InventoryItemSprite => inventoryItemSprite;
        public GameObject ItemPrefab => itemPrefab;
        public bool CanBePickedUp => canBePickedUp;
        public bool CanBeDropped => canBeDropped;
        public bool IsHandItem => isHandItem;
        public bool IsCraftable => isCraftable;
        public IReadOnlyDictionary<ItemType, int> CraftingRequirements => craftingRequirements;

        public bool Equals(Item item)
        {
            return item.ItemType == itemType;
        }

        public bool CanBeCraftedWith(IItemContainer inventory)
        {
            return CraftingRequirements.All(requirement => inventory.HasItem(requirement.Key, requirement.Value));
        }
    }
}