using System.Linq;
using UnityEngine;
using Utilities.RandomService;
using Zenject;

namespace Map
{
    public class MapObjectController : MonoBehaviour
    {
        [SerializeField] private MapObject mapObject;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Inject] private IRandomService randomService;
        
        private void Start()
        {
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if (mapObject.Sprites.Any())
                spriteRenderer.sprite = randomService.Sample(mapObject.Sprites);
        }

        public void SetMapObject(MapObject newMapObject)
        {
            mapObject = newMapObject;
            UpdateSprite();
        }
    }
}