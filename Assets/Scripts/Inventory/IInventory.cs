using Utilities.Monads;

namespace Inventory
{
    public interface IInventory : IItemContainer
    {
        IMaybe<Item> HandItem { get; }
        bool SetHandItem(IMaybe<Item> maybeItem);
    }
}