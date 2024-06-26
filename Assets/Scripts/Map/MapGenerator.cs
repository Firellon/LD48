using System;
using System.Collections.Generic;
using System.Linq;
using Map.Actor;
using Plugins.Sirenix.Odin_Inspector.Modules;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Prefabs;
using Utilities.RandomService;
using Zenject;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        [Inject] private IMapActorRegistry mapActorRegistry;
        [Inject] private IPrefabPool prefabPool;
        [Inject] private IRandomService randomService;
        [Inject] private IMapObjectRegistry mapObjectRegistry;

        [SerializeField] private Vector2Int mapSegmentSize = new(10, 10);

        [ShowInInspector, ReadOnly] private StringToMapSegmentDictionary mapSegmentKeysToMapSegments = new();
        [ShowInInspector, ReadOnly] private CoordinatesToMapSegmentKeyDictionary coordinatesToMapSegmentKeys = new();
        [ShowInInspector, ReadOnly] private HashSet<string> shownMapSegmentKeys = new();
        [ShowInInspector, ReadOnly] private HashSet<string> hiddenMapSegmentKeys = new();
        [ShowInInspector, ReadOnly] private List<Vector2Int> adjacentSegments = new(); // TODO: Remove

        #region Trees

        [SerializeField] private Transform treeParent;
        [SerializeField] private Vector2Int treeDensity = new(2, 2);
        [SerializeField] private float treeSpawnProbability = 0.5f;

        #endregion

        #region Grass

        // TODO: use prefab provider
        [SerializeField] private Transform grassParent;
        [SerializeField] private Vector2Int grassDensity = new(2, 2);
        [SerializeField] private float grassSpawnProbability = 0.5f;

        #endregion

        private const int K_defaultMapSegmentSize = 10;

        private void Awake()
        {
            if (mapSegmentSize.x <= 0) mapSegmentSize.x = K_defaultMapSegmentSize;
            if (mapSegmentSize.y <= 0) mapSegmentSize.y = K_defaultMapSegmentSize;
        }

        private void Update()
        {
            var maybePlayer = mapActorRegistry.Player;
            maybePlayer.IfPresent(player =>
            {
                var playerPosition = player.transform.position;
                var playerIntPosition = new Vector2Int(
                    Mathf.FloorToInt(playerPosition.x / mapSegmentSize.x),
                    Mathf.FloorToInt(playerPosition.y / mapSegmentSize.y)
                );
                var adjacentSegmentCoordinates = GetAdjacentSegmentCoordinates(playerIntPosition);
                adjacentSegments = adjacentSegmentCoordinates;

                ShowAdjacentSegments(adjacentSegmentCoordinates);
                HideNonAdjacentSegments(adjacentSegmentCoordinates);
            });
        }

        private MapSegment CreateMapSegment(Vector2Int segmentPosition)
        {
            var topLeftCorner = segmentPosition * mapSegmentSize;
            var bottomRightCorner = topLeftCorner + mapSegmentSize;
            var trees = GenerateTrees(topLeftCorner, bottomRightCorner);
            var grass = GenerateGrass(topLeftCorner, bottomRightCorner);

            var mapSegment = new MapSegment
            {
                Key = MapSegment.ToMapSegmentCoordinatesKey(segmentPosition),
                StaticObjects = trees.Union(grass).ToList()
            };
            
            coordinatesToMapSegmentKeys.Add(segmentPosition, mapSegment.Key);
            mapSegmentKeysToMapSegments.Add(mapSegment.Key, mapSegment);
            hiddenMapSegmentKeys.Add(mapSegment.Key);
            mapSegment.Hide();

            return mapSegment;
        }

        private MapSegment GetMapSegment(Vector2Int segmentPosition)
        {
            if (!coordinatesToMapSegmentKeys.TryGetValue(segmentPosition, out var segmentKey))
            {
                return CreateMapSegment(segmentPosition);
            }

            if (mapSegmentKeysToMapSegments.TryGetValue(segmentKey, out var mapSegment))
            {
                return mapSegment;
            }

            Debug.LogError($"Data Inconsistency: Key {segmentKey} for position {segmentPosition} exists but the MapSegment does not!");
            
            return CreateMapSegment(segmentPosition);
        }

        private List<Vector2Int> GetAdjacentSegmentCoordinates(Vector2Int segmentPosition)
        {
            var mapSegment = GetMapSegment(segmentPosition);
            return EnumExtensions.GetAllItems<MapSegmentNeighbour>()
                .Select(neighbour =>
                {
                    var neighbourSegmentPosition = segmentPosition + neighbour.GetPosition();
                    if (mapSegment.NeighbourSegmentKeys.TryGetValue(neighbour, out var neighbourKey))
                    {
                        if (!coordinatesToMapSegmentKeys.ContainsKey(neighbourSegmentPosition)) 
                            coordinatesToMapSegmentKeys.Add(neighbourSegmentPosition, neighbourKey);
                        
                        if (coordinatesToMapSegmentKeys[neighbourSegmentPosition] != neighbourKey)
                            Debug.LogError($"GetAdjacentSegmentCoordinates > a key discrepancy for coordinates {neighbourSegmentPosition}: {coordinatesToMapSegmentKeys[neighbourSegmentPosition]} != {neighbourKey}");

                        return neighbourSegmentPosition;
                    }
                    
                    var neighbourMapSegment = GetMapSegment(neighbourSegmentPosition);
                    mapSegment.NeighbourSegmentKeys.Add(neighbour, neighbourMapSegment.Key);

                    return neighbourSegmentPosition;
                }).ToList();
        }

        private List<GameObject> GenerateTrees(Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
        {
            var trees = new List<GameObject>();
            var treeMapObject = mapObjectRegistry.GetMapObject(MapObjectType.Tree);
            for (var treeX = topLeftCorner.x; treeX < bottomRightCorner.x; treeX += treeDensity.x)
            {
                for (var treeY = topLeftCorner.y; treeY < bottomRightCorner.y; treeY += treeDensity.y)
                {
                    if (randomService.Float() > treeSpawnProbability) continue;
                    var treePosition = new Vector2(treeX + randomService.Float(0f, treeDensity.x),
                        treeY + randomService.Float(0f, treeDensity.y));
                    var tree = prefabPool.Spawn(treeMapObject.Prefab, treeParent);
                    tree.GetComponent<MapObjectController>().SetMapObject(treeMapObject);
                    tree.transform.position += new Vector3(treePosition.x, treePosition.y, 0);
                    trees.Add(tree);
                }
            }

            return trees;
        }

        private List<GameObject> GenerateGrass(Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
        {
            var grassObjects = new List<GameObject>();
            var grassMapObject = mapObjectRegistry.GetMapObject(MapObjectType.Grass);
            for (var grassX = topLeftCorner.x; grassX < bottomRightCorner.x; grassX += grassDensity.x)
            {
                for (var grassY = topLeftCorner.y; grassY < bottomRightCorner.y; grassY += grassDensity.y)
                {
                    if (randomService.Float() > grassSpawnProbability) continue;
                    var baseGrassPosition = new Vector2(grassX + randomService.Float(0f, grassDensity.x),
                        grassY + randomService.Float(0f, grassDensity.y));
                    var grassAmount = randomService.Int(1, 9);
                    for (int i = 0; i < grassAmount; i++)
                    {
                        var grass = prefabPool.Spawn(grassMapObject.Prefab, grassParent);
                        grass.GetComponent<MapObjectController>().SetMapObject(grassMapObject);
                        grass.transform.position += new Vector3(baseGrassPosition.x + randomService.Float(-2, 2),
                            baseGrassPosition.y + randomService.Float(-2, 2), 0);
                        grassObjects.Add(grass);
                    }
                }
            }

            return grassObjects;
        }
        
        private void ShowAdjacentSegments(List<Vector2Int> adjacentSegmentCoordinates)
        {
            foreach (var position in adjacentSegmentCoordinates)
            {
                var segmentKey = coordinatesToMapSegmentKeys[position];
                var mapSegment = mapSegmentKeysToMapSegments[segmentKey];
                if (hiddenMapSegmentKeys.Contains(segmentKey))
                {
                    hiddenMapSegmentKeys.Remove(segmentKey);
                    shownMapSegmentKeys.Add(segmentKey);
                    mapSegment.Show();
                }
                else if (!shownMapSegmentKeys.Contains(segmentKey))
                {
                    var newMapSegment = GetMapSegment(position);
                    shownMapSegmentKeys.Add(segmentKey);
                    newMapSegment.Show();
                }
            }
        }

        private void HideNonAdjacentSegments(ICollection<Vector2Int> adjacentSegmentPositions)
        {
            var adjacentSegmentKeys = adjacentSegmentPositions
                    .Select(position => coordinatesToMapSegmentKeys.TryGetValue(position, out var key) ? key : string.Empty)
                    .ToList();
            var keysToRemove = new List<string>();
            foreach (var key in shownMapSegmentKeys)
            {
                if (!adjacentSegmentKeys.Contains(key))
                {
                    keysToRemove.Add(key);
                    hiddenMapSegmentKeys.Add(key);
                    var mapSegment = mapSegmentKeysToMapSegments[key];
                    mapSegment.Hide();
                }
            }

            foreach (var key in keysToRemove)
            {
                shownMapSegmentKeys.Remove(key);
            }
        }
    }

    [Serializable]
    public class CoordinatesToMapSegmentKeyDictionary : UnitySerializedDictionary<Vector2Int, string>
    {
    }

    [Serializable]
    public class StringToMapSegmentDictionary : UnitySerializedDictionary<string, MapSegment>
    {
    }
}