using Dialogue;
using Human;
using Inventory;
using Inventory.UI;
using Map;
using UI;
using UnityEngine;
using Utilities.Monads;

namespace LD48
{
    public interface IInteractable : IHighlightable
    {
        bool CanBePickedUp { get; }
        IMaybe<Item> MaybeItem { get; }
        IMaybe<MapObject> MaybeMapObject { get; }
        GameObject GameObject { get; }
        bool CanInteract();
        void Interact(HumanController humanController);
        void Remove();
    }
}