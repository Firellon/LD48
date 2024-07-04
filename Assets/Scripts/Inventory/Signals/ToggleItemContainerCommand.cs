namespace Inventory.Signals
{
    public class ToggleItemContainerCommand
    {
        public IItemContainer ItemContainer { get; }

        public ToggleItemContainerCommand(IItemContainer itemContainer)
        {
            ItemContainer = itemContainer;
        }
    }

    public class ItemContainerUpdatedEvent
    {
    }
}