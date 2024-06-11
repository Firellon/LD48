using System.Collections.Generic;
using Utilities.Monads;

namespace Map
{
    public interface IMapObjectRegistry
    {
        IMaybe<MapObject> GetMapObjectOrEmpty(MapObjectType objectType);
        MapObject GetMapObject(MapObjectType mapObjectType);
        IReadOnlyList<MapObject> MapObjects { get; }
    }
}