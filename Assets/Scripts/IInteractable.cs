using Human;
using Inventory;
using Map;
using UnityEngine;
using Utilities.Monads;

namespace LD48
{
    public interface IInteractable
    {
        bool CanBePickedUp { get; }
        IMaybe<Item> MaybeItem { get; }
        IMaybe<MapObject> MaybeMapObject { get; }
        GameObject GameObject { get; }
        void SetHighlight(bool isLit);
        bool CanInteract();
        void Interact(HumanController humanController);
        void Remove();
    }
}