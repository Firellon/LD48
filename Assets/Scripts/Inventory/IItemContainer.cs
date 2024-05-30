using System.Collections.Generic;
using LD48;

namespace Inventory
{
    public interface IItemContainer
    {
        public int ItemSlotCount { get; }
        public List<Item> Items { get; }

        public bool CanAddItem();
        public bool AddItem(Item item);
        public bool RemoveItem(Item item);
        public bool HasItem(Item item);
        public bool HasItem(ItemType itemType);
        public bool GetItem(ItemType itemType, out Item item);
        public int GetItemAmount(ItemType itemType);
    }
}