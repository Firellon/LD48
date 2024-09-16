using Human;

namespace Inventory.Signals
{
    public class ShowInventoryCommand
    {
    }

    public class HideInventoryCommand
    {
    }

    public class ToggleInventoryCommand
    {
    }

    public class PlayerInventoryUpdatedEvent
    {
        public PlayerHumanInventory PlayerInventory { get; }
        
        public PlayerInventoryUpdatedEvent(PlayerHumanInventory inventory)
        {
            PlayerInventory = inventory;
        }
    }
}