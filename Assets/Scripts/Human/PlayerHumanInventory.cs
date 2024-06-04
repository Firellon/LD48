using Inventory;
using Inventory.Signals;
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
                SignalsHub.DispatchAsync(new PlayerInventoryUpdatedEvent());
            }
                
            return itemAdded;
        }

        public override bool RemoveItem(Item item)
        {
            var itemRemoved = base.RemoveItem(item);
            if (itemRemoved)
            {
                SignalsHub.DispatchAsync(new PlayerInventoryUpdatedEvent());    
            }
            
            return itemRemoved;
        }
    }
}