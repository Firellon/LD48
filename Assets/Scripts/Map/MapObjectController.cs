using System.Linq;
using UnityEngine;
using Utilities.RandomService;
using Zenject;

namespace Map
{
    public class MapObjectController : MonoBehaviour
    {
        [SerializeField] private MapObject mapObject;

        [Inject] private IRandomService randomService;
        [Inject] private SpriteRenderer spriteRenderer;

        private void Start()
        {
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if (mapObject.Sprites.Any())
                spriteRenderer.sprite = randomService.Sample(mapObject.Sprites);
        }

        public MapObject MapObject => mapObject;

        public void SetMapObject(MapObject newMapObject)
        {
            mapObject = newMapObject;
            UpdateSprite();
        }

        public void Remove()
        {
            throw new System.NotImplementedException();
        }
    }
}