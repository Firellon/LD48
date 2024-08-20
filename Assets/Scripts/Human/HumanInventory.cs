using System;
using System.Collections.Generic;
using Inventory;
using LD48;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Monads;

namespace Human
{
    public class HumanInventory : ItemContainer, IInventory
    {
        [SerializeField] private int itemSlotCount;
        [ShowInInspector, ReadOnly] private List<Item> items = new();
        [ShowInInspector, ReadOnly] private IMaybe<Item> handItem = Maybe.Empty<Item>();

        public override int Capacity => itemSlotCount;
        public override List<Item> Items => items;
        public IMaybe<Item> HandItem => handItem;
        [ShowInInspector, ReadOnly] public bool IsHelpless { get; set; }

        public override bool CanTakeItem()
        {
            return items.Count > 0 && IsHelpless;
        }

        public virtual bool SetHandItem(IMaybe<Item> maybeItem)
        {
            return maybeItem.Match(item =>
            {
                if (!item.IsHandItem) return false;

                handItem = maybeItem;
                return true;
            }, () =>
            {
                handItem = Maybe.Empty<Item>();
                return true;
            });
        }

        public bool MoveHandItemToInventory()
        {
            return handItem.Match(item =>
            {
                if (!CanAddItem()) return false;

                AddItem(item);
                SetHandItem(Maybe.Empty<Item>());
                return true;
            }, true);
        }

        public bool IsHandItem(ItemType itemType)
        {
            return HandItem.Match(item => item.ItemType == itemType, false);
        }
        
        public void IfHandItem(ItemType itemType, Action<Item> some)
        {
            IfHandItem(itemType, some, () => { });
        }

        public void IfHandItem(ItemType itemType, Action<Item> some, Action none)
        {
            HandItem.IfPresent(item =>
            {
                if (item.ItemType == itemType)
                    some(item);
                else
                    none();
            }).IfNotPresent(none);
        }
        
    }
}