using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using LD48;
using Plugins.Sirenix.Odin_Inspector.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Map
{
    [Serializable]
    public class MapSegment
    {
        [ShowInInspector, ReadOnly] public MapSegmentNeighbourToKeyDictionary MapSegmentNeighbourKeys { get; set; } = new();
        [ShowInInspector, ReadOnly] public List<GameObject> MapObjects { get; set; } = new();
        [ShowInInspector, ReadOnly] public List<GameObject> ItemObjects { get; set; } = new();
        [ShowInInspector, ReadOnly] public string Key { get; set; }

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
            foreach (var staticObject in MapObjects)
            {
                staticObject.transform.position += new Vector3(positionDiff.x * mapSegmentSize.x,
                    positionDiff.y * mapSegmentSize.y, 0f);
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
            foreach (var staticObject in MapObjects)
            {
                staticObject.SetActive(true);
            }

            foreach (var itemObject in ItemObjects)
            {
                itemObject.SetActive(true);
            }
        }

        public void Hide()
        {
            foreach (var staticObject in MapObjects)
            {
                staticObject.SetActive(false);
            }
            
            foreach (var itemObject in ItemObjects)
            {
                if (itemObject != null) itemObject.SetActive(false);
            }
        }

        public bool ContainsMapObject<T>() where T : MonoBehaviour
        {
            return MapObjects.Any(staticObject => staticObject.GetComponent<T>());
        }

        public bool ContainsItem(ItemType itemType)
        {
            return ItemObjects.Any(item => item.GetComponent<ItemController>().Item.ItemType == itemType);
        }
    }

    public class MapSegmentNeighbourToKeyDictionary : UnitySerializedDictionary<MapSegmentNeighbour, string>
    {
    }
}