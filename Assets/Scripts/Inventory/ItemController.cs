using LD48;
using UnityEngine;

namespace Inventory
{
    public class ItemController : MonoBehaviour, IInteractable
    {
        [SerializeField] private Item item;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public bool CanBePickedUp => item.CanBePickedUp;
        public Item Item => item;
        public GameObject GameObject => gameObject;

        private void Start()
        {
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            spriteRenderer.sprite = item.ItemSprite;
        }

        public void SetItem(Item newItem)
        {
            item = newItem;
            UpdateSprite();
        }
    }
}