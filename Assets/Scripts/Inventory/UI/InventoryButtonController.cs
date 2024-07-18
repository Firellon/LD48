using Inventory.Signals;
using Signals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Inventory.UI
{
    public class InventoryButtonController : MonoBehaviour
    {
        [Inject] private PlayerInventoryPanelController playerInventoryPanelController;

        [SerializeField] private Button inventoryButton;

        private void OnEnable()
        {
            inventoryButton.onClick.AddListener(ToggleInventory);
            SignalsHub.AddListener<ShowInventoryCommand>(ShowInventory);
            SignalsHub.AddListener<HideInventoryCommand>(HideInventory);
            SignalsHub.AddListener<ToggleInventoryCommand>(OnToggleInventoryCommand);
        }
        
        private void OnDisable()
        {
            inventoryButton.onClick.RemoveListener(ToggleInventory);
            SignalsHub.RemoveListener<ShowInventoryCommand>(ShowInventory);
            SignalsHub.RemoveListener<HideInventoryCommand>(HideInventory);
            SignalsHub.RemoveListener<ToggleInventoryCommand>(OnToggleInventoryCommand);
        }

        private void OnToggleInventoryCommand(ToggleInventoryCommand command)
        {
            ToggleInventory();
        }

        private void ShowInventory(ShowInventoryCommand command)
        {
            playerInventoryPanelController.Show();
        }
        
        private void HideInventory(HideInventoryCommand command)
        {
            playerInventoryPanelController.Hide();
        }

        private void ToggleInventory()
        {
            if (playerInventoryPanelController.IsVisible)
            {
                playerInventoryPanelController.Hide();
            }
            else
            {
                playerInventoryPanelController.Show();
            }
        }
    }
}