using System.Collections.Generic;
using System.Linq;
using LD48;

namespace Inventory
{
    public interface IItemContainer
    {
        public int ItemSlotCount { get; }
        public List<Item> Items { get; }

        public bool CanAddItem()
        {
            return Items.Count < ItemSlotCount;
        }

        public bool AddItem(Item item)
        {
            if (!CanAddItem())
                return false;

            Items.Add(item);
            return true;
        }

        public bool RemoveItem(Item item)
        {
            if (!HasItem(item))
                return false;

            Items.Remove(item);
            return true;
        }

        public bool HasItem(Item item)
        {
            return Items.Find(inventoryItem => inventoryItem.Equals(item));
        }
        
        public bool HasItem(ItemType itemType)
        {
            return Items.Find(inventoryItem => inventoryItem.ItemType == itemType);
        }

        public bool GetItem(ItemType itemType, out Item item)
        {
            item = Items.FirstOrDefault(i => i.ItemType == itemType);
            return item != null;
        }

        public int GetItemAmount(ItemType itemType)
        {
            return Items.Count(item => item.ItemType == itemType);
        }
    }
}