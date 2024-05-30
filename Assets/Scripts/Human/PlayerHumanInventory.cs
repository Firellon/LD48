using Inventory;
using Inventory.Signals;
using Signals;

namespace Human
{
    public class PlayerHumanInventory : HumanInventory
    {
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