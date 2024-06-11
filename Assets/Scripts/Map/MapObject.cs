using UnityEngine;

namespace Map
{
    [CreateAssetMenu(menuName = "Create MapObject", fileName = "MapObject", order = 0)]
    public class MapObject : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private MapObjectType mapObjectType;

        public GameObject Prefab => prefab;
        public MapObjectType ObjectType => mapObjectType;
    }
}