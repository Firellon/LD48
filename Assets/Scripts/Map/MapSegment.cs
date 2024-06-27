using System;
using System.Collections.Generic;
using Plugins.Sirenix.Odin_Inspector.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Map
{
    [Serializable]
    public class MapSegment
    {
        [ShowInInspector] public MapSegmentNeighbourToKeyDictionary MapSegmentNeighbourKeys { get; set; } = new();
        [ShowInInspector] public List<GameObject> StaticObjects { get; set; } = new();
        [ShowInInspector] public string Key { get; set; }

        [ShowInInspector] private Vector2Int position;
        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }

        public void Move(Vector2Int newPosition, Vector2Int mapSegmentSize)
        {
            var positionDiff = newPosition - position;
            if (positionDiff != Vector2Int.zero) Debug.Log($"Move > from position {position} to {newPosition}");
            foreach (var staticObject in StaticObjects)
            {
                staticObject.transform.position += new Vector3(positionDiff.x * mapSegmentSize.x, positionDiff.y * mapSegmentSize.y, 0f);
            }
            position = newPosition;
        }

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