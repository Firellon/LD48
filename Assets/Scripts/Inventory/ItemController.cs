using LD48;
using Map;
using Signals;
using UnityEngine;
using Utilities.Monads;

namespace Inventory
{
    public class ItemController : MonoBehaviour, IInteractable
    {
        [SerializeField] private Item item;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Material regularShader;
        [SerializeField] private Material highlightShader;

        public Item Item => item;
        
        #region IInteractable
        
        public bool CanBePickedUp => item.CanBePickedUp;
        public bool IsItemContainer => false;

        public IMaybe<Item> MaybeItem => item.ToMaybe();

        public IMaybe<MapObject> MaybeMapObject => Maybe.Empty<MapObject>();
        public GameObject GameObject => gameObject;

        public void SetHighlight(bool isLit = true)
        {
            spriteRenderer.material = isLit ? highlightShader : regularShader;
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapItemRemovedEvent(GameObject, item.ItemType));
        }
        
        #endregion

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