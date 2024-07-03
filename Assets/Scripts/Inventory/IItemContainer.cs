using System.Collections.Generic;
using LD48;

namespace Inventory
{
    public interface IItemContainer
    {
        int Capacity { get; }
        List<Item> Items { get; }

        bool CanAddItem();
        bool AddItem(Item item);
        bool RemoveItem(Item item);
        bool RemoveItem(ItemType itemType, int itemAmountToRemove);
        bool HasItem(Item item);
        bool HasItem(ItemType itemType);
        bool HasItem(ItemType itemType, int requiredItemAmount);
        bool GetItem(ItemType itemType, out Item item);
        int GetItemAmount(ItemType itemType);
    }
}