using System.Collections.Generic;
using System.Linq;
using Inventory.Signals;
using Map;
using Map.Actor;
using ModestTree;
using Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Monads;
using Zenject;

namespace Inventory.UI
{
    public class InventoryCraftingItemController : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private Sprite emptyCraftingSprite;

        [SerializeField] private Button craftingItemButton;
        [SerializeField] private Button previousCraftingItemButton;
        [SerializeField] private Button nextCraftingItemButton;

        [Inject] private IItemRegistry itemRegistry;
        [Inject] private IMapActorRegistry mapActorRegistry;

        [ShowInInspector, ReadOnly] private IMaybe<Item> maybeCurrentItem = Maybe.Empty<Item>();
        [ShowInInspector, ReadOnly] private List<Item> craftableItems = new();

        private const int K_noItemIndex = -1;

        private void OnEnable()
        {
            previousCraftingItemButton.onClick.AddListener(OnPreviousCraftableItem);
            craftingItemButton.onClick.AddListener(CraftCurrentItem);
            nextCraftingItemButton.onClick.AddListener(OnNextCraftableItem);
            
            SignalsHub.AddListener<PlayerInventoryUpdatedEvent>(OnPlayerInventoryUpdated);
        }

        private void OnDisable()
        {
            previousCraftingItemButton.onClick.RemoveListener(OnPreviousCraftableItem);
            craftingItemButton.onClick.RemoveListener(CraftCurrentItem);
            nextCraftingItemButton.onClick.RemoveListener(OnNextCraftableItem);
            
            SignalsHub.RemoveListener<PlayerInventoryUpdatedEvent>(OnPlayerInventoryUpdated);
        }

        private void OnPlayerInventoryUpdated(PlayerInventoryUpdatedEvent evt)
        {
            mapActorRegistry.Player.IfPresent(player =>
            {
                UpdateCraftableItems(player.Inventory);
            });
        }

        private void UpdateCraftableItems(IItemContainer playerInventory)
        {
            craftableItems = itemRegistry.Items
                .Where(item => item.IsCraftable && item.CanBeCraftedWith(playerInventory))
                .ToList();
            
            var firstCraftableItem = craftableItems.FirstOrEmpty();
            maybeCurrentItem.IfPresent(currentItem =>
            {
                if (!craftableItems.Contains(currentItem))
                {
                    SetCraftingItem(firstCraftableItem);
                }
            }).IfNotPresent(() =>
            {
                SetCraftingItem(firstCraftableItem);
            });
        }

        private void Start()
        {
            SetCraftingItem(Maybe.Empty<Item>());
        }

        private void SetCraftingItem(IMaybe<Item> maybeItem)
        {
            maybeCurrentItem = maybeItem;
            maybeCurrentItem.IfPresent(item => { itemIcon.sprite = item.InventoryItemSprite; })
                .IfNotPresent(() => { itemIcon.sprite = emptyCraftingSprite; });
        }

        private void OnPreviousCraftableItem()
        {
            if (craftableItems.IsEmpty())
            {
                Debug.Log("OnPreviousCraftableItem > no item to craft!");
                return;
            }

            var currentItemIndex =
                maybeCurrentItem.Match(currentItem => craftableItems.IndexOf(currentItem), K_noItemIndex);
            var previousItemIndex = currentItemIndex == K_noItemIndex ? 0
                : currentItemIndex > 0 ? currentItemIndex - 1 : craftableItems.Count - 1;

            SetCraftingItem(craftableItems[previousItemIndex].ToMaybe());
        }
        
        private void CraftCurrentItem()
        {
            mapActorRegistry.Player.IfPresent(player =>
            {
                maybeCurrentItem.IfPresent(currentItem =>
                {
                    foreach (var (itemType, itemAmount) in currentItem.CraftingRequirements)
                    {
                        player.Inventory.RemoveItem(itemType, itemAmount);
                    }

                    player.Inventory.AddItem(currentItem);

                    UpdateCraftableItems(player.Inventory);
                });
            });
        }

        private void OnNextCraftableItem()
        {
            if (craftableItems.IsEmpty())
            {
                Debug.Log("OnNextCraftableItem > no item to craft!");
                return;
            }

            var currentItemIndex =
                maybeCurrentItem.Match(currentItem => craftableItems.IndexOf(currentItem), K_noItemIndex);
            var nextItemIndex = currentItemIndex == K_noItemIndex ? 0
                : currentItemIndex < craftableItems.Count - 1 ? currentItemIndex + 1 : 0;

            SetCraftingItem(craftableItems[nextItemIndex].ToMaybe());
        }
    }
}