using System;
using System.Collections.Generic;
using Plugins.Sirenix.Odin_Inspector.Modules;
using UnityEngine;

namespace Map
{
    [Serializable]
    public class MapSegment
    {
        public MapSegmentNeighbourToKeyDictionary NeighbourSegmentKeys { get; private set; } = new();
        public List<GameObject> StaticObjects { get; set; } = new();
        public string Key { get; set; }

        private const string K_keySeparator = "_";
        public static string ToMapSegmentCoordinatesKey(Vector2Int position)
        {
            return $"{position.x}{K_keySeparator}{position.y}";
        }
        
        public void Show()
        {
            foreach (var staticObject in StaticObjects)
            {
               staticObject.SetActive(true); 
            }
        }
        
        public void Hide()
        {
            foreach (var staticObject in StaticObjects)
            {
                staticObject.SetActive(false); 
            }
        }
    }

    public class MapSegmentNeighbourToKeyDictionary : UnitySerializedDictionary<MapSegmentNeighbour, string>
    {
    }
}