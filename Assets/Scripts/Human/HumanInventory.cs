using System.Collections.Generic;
using System.Linq;
using Inventory;
using Inventory.Signals;
using LD48;
using Signals;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Human
{
    public class HumanInventory : MonoBehaviour, IItemContainer
    {
        [SerializeField] private int itemSlotCount;
        [ShowInInspector, ReadOnly] private List<Item> items = new();

        public int ItemSlotCount => itemSlotCount;
        public List<Item> Items => items;
        
        public virtual bool CanAddItem()
        {
            return Items.Count < ItemSlotCount;
        }

        public virtual bool AddItem(Item item)
        {
            if (!CanAddItem())
                return false;

            Items.Add(item);
            
            return true;
        }

        public virtual bool RemoveItem(Item item)
        {
            if (!HasItem(item))
                return false;

            Items.Remove(item);
            return true;
        }

        public virtual bool HasItem(Item item)
        {
            return Items.Find(inventoryItem => inventoryItem.Equals(item));
        }
        
        public virtual bool HasItem(ItemType itemType)
        {
            return Items.Find(inventoryItem => inventoryItem.ItemType == itemType);
        }

        public virtual bool GetItem(ItemType itemType, out Item item)
        {
            item = Items.FirstOrDefault(i => i.ItemType == itemType);
            return item != null;
        }

        public virtual int GetItemAmount(ItemType itemType)
        {
            return Items.Count(item => item.ItemType == itemType);
        }
    }
}