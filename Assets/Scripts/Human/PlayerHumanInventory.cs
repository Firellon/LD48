using Inventory;
using Inventory.Signals;
using LD48;
using Signals;
using Utilities.Monads;

namespace Human
{
    public class PlayerHumanInventory : HumanInventory
    {
        public override bool SetHandItem(IMaybe<Item> maybeItem)
        {
            var itemSet = base.SetHandItem(maybeItem);
            if (itemSet)
            {
                SignalsHub.DispatchAsync(new PlayerHandItemUpdatedEvent(maybeItem));
            }

            return itemSet;
        }

        public override bool AddItem(Item item)
        {
            var itemAdded = base.AddItem(item);
            if (itemAdded)
            {
                SignalsHub.DispatchAsync(new PlayerInventoryUpdatedEvent(this));
            }

            return itemAdded;
        }

        public override bool RemoveItem(Item item)
        {
            var itemRemoved = base.RemoveItem(item);
            if (itemRemoved)
            {
                SignalsHub.DispatchAsync(new PlayerInventoryUpdatedEvent(this));
            }

            return itemRemoved;
        }

        public override bool RemoveItem(ItemType itemType, int itemAmountToRemove)
        {
            if (!GetItem(itemType, out var itemToRemove))
            {
                return false;
            }

            for (var i = 0; i < itemAmountToRemove; i++)
            {
                if (!RemoveItem(itemToRemove)) return false;
            }

            return true;
        }
    }
}