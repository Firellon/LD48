using System.Collections.Generic;
using Inventory;
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
        public bool IsHelpless { get; set; }

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
    }
}