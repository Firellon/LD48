using System;
using Inventory.UI;
using Signals;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Inventory
{
    public class InventoryButtonController : MonoBehaviour
    {
        [Inject] private IInventoryPanelController inventoryPanelController;

        [SerializeField] private Button inventoryButton;

        private void OnEnable()
        {
            inventoryButton.onClick.AddListener(ToggleInventory);
            SignalsHub.AddListener<ToggleInventoryCommand>(OnToggleInventoryCommand);
        }

        private void OnToggleInventoryCommand(ToggleInventoryCommand obj)
        {
            ToggleInventory();
        }

        private void OnDisable()
        {
            inventoryButton.onClick.RemoveListener(ToggleInventory);
            SignalsHub.RemoveListener<ToggleInventoryCommand>(OnToggleInventoryCommand);
        }

        private void ToggleInventory()
        {
            if (inventoryPanelController.IsVisible)
            {
                inventoryPanelController.Hide();
            }
            else
            {
                inventoryPanelController.Show();
            }
        }
    }

    public class ToggleInventoryCommand
    {
    }
}