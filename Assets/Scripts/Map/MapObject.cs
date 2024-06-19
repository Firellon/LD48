using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [CreateAssetMenu(menuName = "LD48/Create MapObject SO", fileName = "New MapObject", order = 0)]
    public class MapObject : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private MapObjectType mapObjectType;
        [SerializeField] private List<Sprite> sprites = new();

        public GameObject Prefab => prefab;
        public MapObjectType ObjectType => mapObjectType;
        public IList<Sprite> Sprites => sprites;
    }
}