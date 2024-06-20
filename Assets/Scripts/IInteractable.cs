using Inventory;
using UnityEngine;

namespace LD48
{
    public interface IInteractable
    {
        bool CanBePickedUp { get; }
        Item Item { get; }
        
        GameObject GameObject { get; }
        void SetHighlight(bool isLit);
    }
}