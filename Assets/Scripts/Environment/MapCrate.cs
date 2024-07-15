using System.Collections.Generic;
using Inventory;
using LD48;
using Map;
using Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Monads;

namespace Environment
{
    public class MapCrate : ItemContainer, IInteractable
    {
        [SerializeField] private int capacity = 16;
        [ShowInInspector, ReadOnly] private List<Item> items = new();

        [Space]
        [SerializeField] private MapObjectController mapObjectController; // TODO: Inject
        [SerializeField] private SpriteRenderer spriteRenderer; // TODO: inject
        [SerializeField] private Material regularShader;
        [SerializeField] private Material highlightShader;

        public override int Capacity => capacity;
        public override List<Item> Items => items;

        #region IInteractable

        public bool CanBePickedUp => false;
        public bool IsItemContainer => true;

        public IMaybe<Item> MaybeItem => Maybe.Empty<Item>();

        public IMaybe<MapObject> MaybeMapObject => mapObjectController.MapObject.ToMaybe();
        public GameObject GameObject => gameObject;
        public void SetHighlight(bool isLit)
        {
            spriteRenderer.material = isLit ? highlightShader : regularShader;
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }

        #endregion
    }
}