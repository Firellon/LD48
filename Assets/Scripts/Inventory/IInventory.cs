using Utilities.Monads;

namespace Inventory
{
    public interface IInventory : IItemContainer
    {
        bool IsHelpless { set; }
        IMaybe<Item> HandItem { get; }
        bool SetHandItem(IMaybe<Item> maybeItem);
        bool MoveHandItemToInventory();
    }
}