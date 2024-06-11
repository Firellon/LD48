using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.Monads;

namespace Map
{
    public class MapObjectRegistry : MonoBehaviour, IMapObjectRegistry
    {
        [SerializeField] private List<MapObject> mapObjects = new();

        public IMaybe<MapObject> GetMapObjectOrEmpty(MapObjectType objectType)
        {
            return mapObjects.FirstOrEmpty(mapObject => mapObject.ObjectType == objectType);
        }

        public MapObject GetMapObject(MapObjectType objectType)
        {
            return mapObjects.First(item => item.ObjectType == objectType);
        }

        public IReadOnlyList<MapObject> MapObjects => mapObjects;
    }
}