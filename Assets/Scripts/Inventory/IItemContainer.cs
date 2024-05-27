using System.Collections.Generic;

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
            if (CanAddItem())
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
    }
}