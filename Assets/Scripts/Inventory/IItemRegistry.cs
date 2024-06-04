using LD48;
using Utilities.Monads;

namespace Inventory
{
    public interface IItemRegistry
    {
        IMaybe<Item> GetItem(ItemType itemType);
    }
}