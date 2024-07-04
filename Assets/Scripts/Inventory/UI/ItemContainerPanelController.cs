using System;
using LD48;
using UnityEngine;
using Utilities;
using Utilities.Prefabs;
using Zenject;

namespace Inventory.UI
{
    public class ItemContainerPanelController : MonoBehaviour, IItemContainerPanelController
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject inventorySlotPrefab;

        [Inject] private IPrefabPool prefabPool;

        private IItemContainer _itemContainer;

        private void Start()
        {
            inventoryPanel.SetActive(false);
        }

        public void Show()
        {
            if (IsVisible) return;

            inventoryPanel.SetActive(true);
        }

        public void Hide()
        {
            if (!IsVisible) return;

            inventoryPanel.SetActive(false);
        }

        public bool IsVisible => inventoryPanel.activeSelf;

        public void SetUp(IItemContainer itemContainer)
        {
            _itemContainer = itemContainer;
            UpdateInventoryPanelItems();
        }

        private void UpdateInventoryPanelItems()
        {
            inventoryPanel.transform.DespawnChildren(prefabPool);

            for (var i = 0; i < _itemContainer.Capacity; i++)
            {
                var itemSlotView = prefabPool.Spawn(inventorySlotPrefab, inventoryPanel.transform)
                    .GetComponent<InventoryItemView>();
                itemSlotView.SetUp(_itemContainer.Items.GetElementByIndexOrEmpty(i));
            }
        }
    }
}