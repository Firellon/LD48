using Map;
using UnityEngine;

namespace Inventory.Signals
{
    public class MapObjectAddedEvent
    {
        public GameObject GameObject { get; }
        public MapObjectType MapObjectType { get; }

        public MapObjectAddedEvent(GameObject gameObject, MapObjectType mapObjectType)
        {
            GameObject = gameObject;
            MapObjectType = mapObjectType;
        }
    }
}