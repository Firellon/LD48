using LD48;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(menuName = "Create Item SO", fileName = "New Item", order = 0)]
    public class Item : ScriptableObject
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private Sprite itemSprite;
        [SerializeField] private bool canBePickedUp;

        public ItemType ItemType => itemType;
        public Sprite ItemSprite => itemSprite;
        public bool CanBePickedUp => canBePickedUp;

        public bool Equals(Item item)
        {
            return item.ItemType == itemType;
        }
    }
}