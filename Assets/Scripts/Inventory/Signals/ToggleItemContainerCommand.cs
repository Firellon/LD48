using UnityEngine;

namespace Inventory.Signals
{
    public class ToggleItemContainerCommand
    {
        public IItemContainer ItemContainer { get; }
        public GameObject GameObject { get; }

        public ToggleItemContainerCommand(IItemContainer itemContainer, GameObject gameObject)
        {
            ItemContainer = itemContainer;
            GameObject = gameObject;
        }
    }

    public class ItemContainerUpdatedEvent
    {
    }
}