using Human;
using Inventory.Signals;
using Signals;
using UnityEngine;
using Zenject;

namespace Inventory.UI
{
    public class InteractableInventoryController : MonoBehaviour
    {
        [Inject] private ItemContainerPanelController itemContainerPanelController;

        private GameObject _currentInteractable;

        private void OnEnable()
        {
            SignalsHub.AddListener<ToggleItemContainerCommand>(ToggleItemContainer);
            SignalsHub.AddListener<InteractableExitEvent>(OnInteractableExit);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<ToggleItemContainerCommand>(ToggleItemContainer);
            SignalsHub.RemoveListener<InteractableExitEvent>(OnInteractableExit);
        }

        private void ToggleItemContainer(ToggleItemContainerCommand command)
        {
            _currentInteractable = command.GameObject;
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

        private void OnInteractableExit(InteractableExitEvent evt)
        {
            if (_currentInteractable == evt.Interactable.GameObject)
                itemContainerPanelController.Hide();
        }
    }
}