using System.Collections.Generic;
using LD48;
using Utilities.Monads;

namespace Inventory
{
    public interface IItemRegistry
    {
        IMaybe<Item> GetItem(ItemType itemType);
        IReadOnlyList<Item> Items { get; }
    }
}