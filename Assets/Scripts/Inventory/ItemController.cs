using LD48;
using UnityEngine;

namespace Inventory
{
    public class ItemController : MonoBehaviour, IInteractable
    {
        [SerializeField] private Item item;
        
        public bool CanBePickedUp => item.CanBePickedUp;
        public Item Item => item;
        public GameObject GameObject => gameObject;
    }
}