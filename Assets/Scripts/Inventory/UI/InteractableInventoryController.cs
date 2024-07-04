using System;
using Inventory.Signals;
using Signals;
using UnityEngine;
using Zenject;

namespace Inventory.UI
{
    public class InteractableInventoryController : MonoBehaviour
    {
        [Inject] private ItemContainerPanelController itemContainerPanelController;
        
        private void OnEnable()
        {
            SignalsHub.AddListener<ToggleItemContainerCommand>(ToggleItemContainer);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<ToggleItemContainerCommand>(ToggleItemContainer);
        }

        private void ToggleItemContainer(ToggleItemContainerCommand command)
        {
            itemContainerPanelController.SetUp(command.ItemContainer);
            if (itemContainerPanelController.IsVisible)
            {
                itemContainerPanelController.Hide();
            }
            else
            {
                itemContainerPanelController.Show();
            }
        }
    }
}