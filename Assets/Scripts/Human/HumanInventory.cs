using System.Collections.Generic;
using System.Linq;
using Inventory;
using LD48;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Monads;

namespace Human
{
    public class HumanInventory : MonoBehaviour, IItemContainer
    {
        [SerializeField] private int itemSlotCount;
        [ShowInInspector, ReadOnly] private List<Item> items = new();
        [ShowInInspector, ReadOnly] private IMaybe<Item> handItem = Maybe.Empty<Item>();

        public int ItemSlotCount => itemSlotCount;
        public List<Item> Items => items;
        public IMaybe<Item> HandItem => handItem;

        public virtual bool SetHandItem(IMaybe<Item> maybeItem)
        {
            return maybeItem.Match(item =>
            {
                if (!item.CanUse) return false;

                handItem = maybeItem;
                return true;
            }, () =>
            {
                handItem = Maybe.Empty<Item>();
                return true;
            });
        }

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