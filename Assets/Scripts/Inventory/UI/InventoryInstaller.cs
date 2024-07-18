using UnityEngine;
using Zenject;

namespace Inventory.UI
{
    public class InventoryInstaller : MonoInstaller<InventoryInstaller>
    {
        [SerializeField] private InventoryButtonController inventoryButton;
        [SerializeField] private InventoryHandItemController inventoryHandItem;
        [SerializeField] private InventoryCraftingItemController inventoryCraftingItem;
        [SerializeField] private PlayerInventoryPanelController playerInventoryPanelController;
        [SerializeField] private ItemContainerPanelController itemContainerPanelController;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InventoryButtonController>().FromInstance(inventoryButton);
            Container.BindInterfacesAndSelfTo<PlayerInventoryPanelController>()
                .FromInstance(playerInventoryPanelController);
            Container.BindInterfacesAndSelfTo<InventoryHandItemController>().FromInstance(inventoryHandItem);
            Container.BindInterfacesAndSelfTo<InventoryCraftingItemController>().FromInstance(inventoryCraftingItem);
            Container.BindInterfacesAndSelfTo<ItemContainerPanelController>()
                .FromInstance(itemContainerPanelController);
        }
    }
}