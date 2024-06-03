using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Inventory.UI
{
    public class InventoryInstaller : MonoInstaller<InventoryInstaller>
    {
        [SerializeField] private InventoryButtonController inventoryButton;
        [FormerlySerializedAs("inventoryHandZone")] [SerializeField] private InventoryHandItemController inventoryHandItem;
        [SerializeField] private PlayerInventoryPanelController playerInventoryPanelController;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InventoryButtonController>().FromInstance(inventoryButton);
            Container.BindInterfacesAndSelfTo<PlayerInventoryPanelController>().FromInstance(playerInventoryPanelController);
            Container.BindInterfacesAndSelfTo<InventoryHandItemController>().FromInstance(inventoryHandItem);
        }
    }
}