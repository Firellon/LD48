using System.Collections.Generic;
using System.Linq;
using LD48;
using UnityEngine;
using Utilities.Monads;

namespace Inventory
{
    public class ItemRegistry : MonoBehaviour, IItemRegistry
    {
        [SerializeField] private List<Item> items = new();

        public IMaybe<Item> GetItemOrEmpty(ItemType itemType)
        {
            return items.FirstOrEmpty(item => item.ItemType == itemType);
        }

        public Item GetItem(ItemType itemType)
        {
            return items.First(item => item.ItemType == itemType);
        }

        public IReadOnlyList<Item> Items => items;
    }
}