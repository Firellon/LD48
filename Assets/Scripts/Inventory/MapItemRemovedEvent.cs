using LD48;
using UnityEngine;

namespace Inventory
{
    public class MapItemRemovedEvent
    {
        public GameObject GameObject { get; }
        public ItemType ItemType { get; }
        
        public MapItemRemovedEvent( GameObject gameObject, ItemType itemType)
        {
            GameObject = gameObject;
            ItemType = itemType;
        }
    }
}