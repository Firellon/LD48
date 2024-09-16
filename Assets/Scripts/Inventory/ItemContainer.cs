using System.Collections.Generic;
using System.Linq;
using LD48;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Inventory
{
    public abstract class ItemContainer : MonoBehaviour, IItemContainer
    {
        public Transform Transform => transform;
        public abstract int Capacity { get; }
        public abstract List<Item> Items { get; }
        public virtual UnityEvent ItemsUpdatedEvent { get; } = new UnityEvent();

        public virtual bool CanTakeItem()
        {
            return Items.Count > 0;
        }
        
        public bool CanAddItem()
        {
            return Items.Count < Capacity;
        }

        public virtual bool AddItem(Item item)
        {
            if (!CanAddItem())
                return false;

            Items.Add(item);

            ItemsUpdatedEvent.Invoke();
            return true;
        }

        public virtual bool RemoveItem(Item item)
        {
            if (!HasItem(item))
                return false;

            Items.Remove(item);
            
            ItemsUpdatedEvent.Invoke();
            return true;
        }

        public virtual bool RemoveItem(ItemType itemType, int itemAmountToRemove)
        {
            if (!GetItem(itemType, out var itemToRemove))
            {
                return false;
            }
            
            for (var i = 0; i < itemAmountToRemove; i++)
            {
                if (!RemoveItem(itemToRemove)) return false;
            }

            ItemsUpdatedEvent.Invoke();
            return true;
        }
        public virtual void RemoveItems()
        {
            Items.Clear();
            ItemsUpdatedEvent.Invoke();
        }

        public bool HasItem(Item item)
        {
            return Items.Find(inventoryItem => inventoryItem.Equals(item));
        }

        public bool HasItem(ItemType itemType)
        {
            return Items.Find(inventoryItem => inventoryItem.ItemType == itemType);
        }

        public bool HasItem(ItemType itemType, int requiredItemAmount)
        {
            return Items.Count(item => item.ItemType == itemType) >= requiredItemAmount;
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