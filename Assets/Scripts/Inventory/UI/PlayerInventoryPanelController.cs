using System;
using DI;
using Human;
using Inventory.Signals;
using LD48;
using Map;
using Map.Actor;
using Signals;
using UnityEngine;
using Utilities;
using Utilities.Prefabs;
using Zenject;

namespace Inventory.UI
{
    public class PlayerInventoryPanelController : MonoBehaviour, IItemContainerPanelController
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject inventorySlotPrefab;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IMapActorRegistry mapActorRegistry;

        private void OnEnable()
        {
            SignalsHub.AddListener<PlayerInventoryUpdatedEvent>(OnPlayerInventoryUpdated);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayerInventoryUpdatedEvent>(OnPlayerInventoryUpdated);
        }

        private void OnPlayerInventoryUpdated(PlayerInventoryUpdatedEvent evt)
        {
            if (!IsVisible) return;

            UpdateInventoryPanelItems();
        }

        private void Start()
        {
            inventoryPanel.SetActive(false);
        }

        public void Show()
        {
            if (IsVisible) return;

            inventoryPanel.SetActive(true);

            UpdateInventoryPanelItems();
        }

        public void Hide()
        {
            if (!IsVisible) return;

            inventoryPanel.SetActive(false);
        }

        public bool IsVisible => inventoryPanel.activeSelf;
        public void SetUp(IItemContainer itemContainer)
        {
            throw new NotImplementedException();
        }

        private void UpdateInventoryPanelItems()
        {
            inventoryPanel.transform.DespawnChildren(prefabPool);
            mapActorRegistry.Player.IfPresent(player =>
            {
                for (var i = 0; i < player.Inventory.Capacity; i++)
                {
                    var itemSlotView = prefabPool.Spawn(inventorySlotPrefab, inventoryPanel.transform)
                        .GetComponent<PlayerInventoryItemView>();
                    itemSlotView.SetUp(player.Inventory.Items.GetElementByIndexOrEmpty(i), player);
                }
            });
        }
    }
}