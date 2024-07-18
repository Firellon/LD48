using System.Collections.Generic;
using Human;
using Inventory;
using Inventory.Signals;
using LD48;
using Map;
using Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Monads;
using Zenject;

namespace Environment
{
    public class MapCrate : ItemContainer, IInteractable
    {
        [SerializeField] private int capacity = 16;
        [ShowInInspector, ReadOnly] private List<Item> items = new();

        [Inject] private MapObjectController mapObjectController;
        [Inject] private SpriteRenderer spriteRenderer;
        [Inject] private VisualsConfig visualsConfig;

        public override int Capacity => capacity;
        public override List<Item> Items => items;

        #region IInteractable

        public bool CanBePickedUp => false;

        public IMaybe<Item> MaybeItem => Maybe.Empty<Item>();

        public IMaybe<MapObject> MaybeMapObject => mapObjectController.MapObject.ToMaybe();
        public GameObject GameObject => gameObject;

        public void SetHighlight(bool isLit)
        {
            spriteRenderer.material =
                isLit ? visualsConfig.HighlightedInteractableShader : visualsConfig.RegularInteractableShader;
        }

        public void Interact(HumanController humanController)
        {
            SignalsHub.DispatchAsync(new ToggleItemContainerCommand(this, GameObject));
            SignalsHub.DispatchAsync(new ShowInventoryCommand());
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }

        #endregion
    }
}