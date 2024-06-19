using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Map.Actor;
using Plugins.Sirenix.Odin_Inspector.Modules;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Prefabs;
using Utilities.RandomService;
using Zenject;
using Random = System.Random;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        [Inject] private IMapActorRegistry mapActorRegistry;
        [Inject] private IPrefabPool prefabPool;
        [Inject] private IRandomService randomService;
        [Inject] private IMapObjectRegistry mapObjectRegistry;

        [SerializeField] private Vector2Int mapSegmentSize = new(10, 10);

        [ShowInInspector, ReadOnly] private CoordinatesToMapSegmentDictionary shownMapSegmentDictionary = new();
        [ShowInInspector, ReadOnly] private CoordinatesToMapSegmentDictionary hiddenMapSegmentDictionary = new();
        [ShowInInspector, ReadOnly] private List<Vector2Int> _adjacentSegments = new(); // TODO: Remove

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
                    Mathf.FloorToInt(playerPosition.x / mapSegmentSize.x),
                    Mathf.FloorToInt(playerPosition.y / mapSegmentSize.y)
                );
                var adjacentSegments = GetAdjacentSegments(playerMapSegment);
                _adjacentSegments = adjacentSegments;

                ShowAdjacentSegments(adjacentSegments);
                HideNonAdjacentSegments(adjacentSegments);
            });
        }

        private void ShowAdjacentSegments(List<Vector2Int> adjacentSegments)
        {
            foreach (var adjacentSegmentCoordinates in adjacentSegments)
            {
                var key = ToMapSegmentCoordinatesKey(adjacentSegmentCoordinates);
                if (hiddenMapSegmentDictionary.ContainsKey(key))
                {
                    var mapSegment = hiddenMapSegmentDictionary[key];
                    hiddenMapSegmentDictionary.Remove(key);
                    shownMapSegmentDictionary.Add(key, mapSegment);
                    mapSegment.Show();
                }
                else if (!shownMapSegmentDictionary.ContainsKey(key))
                {
                    var newMapSegment = CreateMapSegment(adjacentSegmentCoordinates);
                    shownMapSegmentDictionary.Add(key, newMapSegment);
                }
            }
        }

        private MapSegment CreateMapSegment(Vector2Int adjacentSegmentCoordinates)
        {
            var topLeftCorner = adjacentSegmentCoordinates * mapSegmentSize;
            var bottomRightCorner = topLeftCorner + mapSegmentSize;
            var trees = GenerateTrees(topLeftCorner, bottomRightCorner);
            var grass = GenerateGrass(topLeftCorner, bottomRightCorner);
            
            return new MapSegment
            {
                Coordinates = adjacentSegmentCoordinates,
                StaticObjects = trees.Union(grass).ToList()
            };
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
                        grass.transform.position += new Vector3(baseGrassPosition.x + randomService.Float(-2, 2), baseGrassPosition.y + randomService.Float(-2, 2), 0);
                        grassObjects.Add(grass);    
                    }
                }
            }

            return grassObjects;
        }

        private void HideNonAdjacentSegments(List<Vector2Int> adjacentSegments)
        {
            var keysToRemove = new List<string>();
            foreach (var key in shownMapSegmentDictionary.Keys)
            {
                var mapSegment = shownMapSegmentDictionary[key];
                var keyCoordinates = FromMapSegmentCoordinatesKey(key);
                if (!adjacentSegments.Contains(keyCoordinates))
                {
                    keysToRemove.Add(key);
                    hiddenMapSegmentDictionary.Add(key, mapSegment);
                    mapSegment.Hide();
                }
            }

            foreach (var key in keysToRemove)
            {
                shownMapSegmentDictionary.Remove(key);
            }
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