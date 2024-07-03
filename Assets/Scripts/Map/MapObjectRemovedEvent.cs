using UnityEngine;

namespace Map
{
    public class MapObjectRemovedEvent
    {
        public GameObject GameObject { get; }
        public MapObjectType ObjectType { get; }
        
        public MapObjectRemovedEvent( GameObject gameObject, MapObjectType objectType)
        {
            GameObject = gameObject;
            ObjectType = objectType;
        }
    }
}