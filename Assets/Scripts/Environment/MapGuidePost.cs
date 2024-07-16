using Inventory;
using LD48;
using Map;
using Signals;
using UnityEngine;
using Utilities.Monads;
using Zenject;

namespace Environment
{
    public class MapGuidePost : MonoBehaviour, IInteractable
    {
        [SerializeField] private MapObjectController mapObjectController; // TODO: Inject
        [SerializeField] private SpriteRenderer spriteRenderer; // TODO: Inject

        [Inject] private VisualsConfig visualsConfig;

        public bool CanBePickedUp => false;
        public bool IsItemContainer => false;
        public IMaybe<Item> MaybeItem => Maybe.Empty<Item>();
        public IMaybe<MapObject> MaybeMapObject => mapObjectController.MapObject.ToMaybe();
        public GameObject GameObject => gameObject;

        public void SetHighlight(bool isLit)
        {
            spriteRenderer.material = isLit ? visualsConfig.HighlightedInteractableShader : visualsConfig.RegularInteractableShader;
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }
    }
}