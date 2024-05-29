using System;
using DI;
using Human;
using LD48;
using Map;
using UnityEngine;
using Utilities;
using Utilities.Prefabs;
using Zenject;

namespace Inventory.UI
{
    public class PlayerInventoryPanelController : MonoBehaviour, IInventoryPanelController
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject inventorySlotPrefab;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IMapActorRegistry mapActorRegistry;

        private void Start()
        {
            inventoryPanel.SetActive(false);
        }

        public void Show()
        {
            if (IsVisible) return;

            inventoryPanel.transform.DespawnChildren(prefabPool);
            inventoryPanel.SetActive(true);

            mapActorRegistry.Player.IfPresent(player =>
            {
                for (var i = 0; i < player.Inventory.ItemSlotCount; i++)
                {
                    var itemSlotView = prefabPool.Spawn(inventorySlotPrefab, inventoryPanel.transform)
                        .GetComponent<InventoryItemView>();
                    itemSlotView.SetUp(player.Inventory.Items.GetElementByIndexOrEmpty(i));
                }
            });
        }

        public void Hide()
        {
            if (!IsVisible) return;

            inventoryPanel.SetActive(false);
        }

        public bool IsVisible => inventoryPanel.activeSelf;
    }
}