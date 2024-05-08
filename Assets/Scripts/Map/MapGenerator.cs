using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plugins.Sirenix.Odin_Inspector.Modules;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        [Inject] private IMapActorRegistry mapActorRegistry;

        [SerializeField] private Vector2Int mapSegmentSize = new(10, 10);

        [ShowInInspector, ReadOnly] private CoordinatesToMapSegmentDictionary MapSegmentDictionary = new();
        [ShowInInspector, ReadOnly] private List<Vector2Int> _adjacentSegments = new(); // TODO: Remove

        private void Awake()
        {
            if (mapSegmentSize.x <= 0) mapSegmentSize.x = 10;
            if (mapSegmentSize.y <= 0) mapSegmentSize.y = 10;
        }

        private void Update()
        {
            var maybePlayer = mapActorRegistry.Player;
            maybePlayer.IfPresent(player =>
            {
                var playerPosition = player.transform.position;
                var playerMapSegment = new Vector2Int(
                    Mathf.RoundToInt(playerPosition.x / mapSegmentSize.x),
                    Mathf.RoundToInt(playerPosition.y / mapSegmentSize.y)
                );
                var adjacentSegments = GetAdjacentSegments(playerMapSegment);
                _adjacentSegments = adjacentSegments;
            });
        }

        private List<Vector2Int> GetAdjacentSegments(Vector2Int playerMapSegment)
        {
            return new List<Vector2Int>
            {
                playerMapSegment + Vector2Int.left + Vector2Int.down,
                playerMapSegment + Vector2Int.down,
                playerMapSegment + Vector2Int.right + Vector2Int.down,
                playerMapSegment + Vector2Int.left,
                playerMapSegment,
                playerMapSegment + Vector2Int.right,
                playerMapSegment + Vector2Int.left + Vector2Int.up,
                playerMapSegment + Vector2Int.up,
                playerMapSegment + Vector2Int.right + Vector2Int.up,
            };
        }

        private void GenerateMapSegment(Vector2 mapSegmentPosition, Vector2 size)
        {
        }

        private const string K_keySeparator = "_";

        public static string ToMapSegmentCoordinatesKey(Vector2Int coords)
        {
            return $"{coords.x}{K_keySeparator}{coords.y}";
        }

        public static Vector2Int FromMapSegmentCoordinatesKey(string key)
        {
            var splitCoords = key.Split(K_keySeparator).Select(int.Parse).ToArray();
            if (splitCoords.Length < 2)
                throw new InvalidDataException($"FromMapSegmentCoordinatesKey > invalid key [ {key} ]");

            return new Vector2Int(splitCoords[0], splitCoords[1]);
        }
    }

    [Serializable]
    public class CoordinatesToMapSegmentDictionary : UnitySerializedDictionary<string, MapSegment>
    {
    }
}