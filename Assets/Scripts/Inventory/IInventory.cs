using System;
using LD48;
using Utilities.Monads;

namespace Inventory
{
    public interface IInventory : IItemContainer
    {
        bool IsHelpless { set; }
        IMaybe<Item> HandItem { get; }
        bool SetHandItem(IMaybe<Item> maybeItem);
        bool MoveHandItemToInventory();
        void IfHandItem(ItemType itemType, Action<Item> some, Action none);
        void IfHandItem(ItemType itemType, Action<Item> some);
        bool IsHandItem(ItemType itemType);
    }
}