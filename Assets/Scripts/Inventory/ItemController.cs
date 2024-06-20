using LD48;
using UnityEngine;

namespace Inventory
{
    public class ItemController : MonoBehaviour, IInteractable
    {
        [SerializeField] private Item item;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Material regularShader;
        [SerializeField] private Material highlightShader;

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

        public void SetHighlight(bool isLit = true)
        {
            spriteRenderer.material = isLit ? highlightShader : regularShader;
        }
    }
}