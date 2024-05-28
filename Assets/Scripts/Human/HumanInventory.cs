using System.Collections.Generic;
using Inventory;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Human
{
    public class HumanInventory : MonoBehaviour, IItemContainer
    {
        [SerializeField] private int itemSlotCount;
        [ShowInInspector, ReadOnly] private List<Item> items = new();

        public int ItemSlotCount => itemSlotCount;
        public List<Item> Items => items;
    }
}