﻿using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Environment;
using Inventory;
using LD48;
using Map.Actor;
using Plugins.Sirenix.Odin_Inspector.Modules;
using Signals;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Utilities;
using Utilities.Monads;
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
        [Inject] private IItemRegistry itemRegistry;

        [SerializeField] private Vector2Int halfMapSize = new(20, 20);
        [SerializeField] private Vector2Int mapSegmentSize = new(10, 10);

        [ShowInInspector, ReadOnly] private StringToMapSegmentDictionary mapSegmentKeysToMapSegments = new();
        [ShowInInspector, ReadOnly] private Vector2IntToStringDictionary positionsToMapSegmentKeys = new();
        [ShowInInspector, ReadOnly] private HashSet<string> shownMapSegmentKeys = new();
        [ShowInInspector, ReadOnly] private HashSet<string> hiddenMapSegmentKeys = new();
        [ShowInInspector, ReadOnly] private List<Vector2Int> adjacentSegments = new(); // TODO: Remove

        #region Trees

        [SerializeField] private Transform treeParent;
        [SerializeField] private Vector2Int treeDensity = new(2, 2);
        [SerializeField] private float treeSpawnProbability = 0.5f;

        #endregion

        #region Grass

        [SerializeField] private Transform grassParent;
        [SerializeField] private Vector2Int grassDensity = new(2, 2);
        [SerializeField] private float grassSpawnProbability = 0.5f;

        #endregion

        #region Segments

        /**
         * How Many empty segments should a segment have to warrant a gate creation?
         */
        [SerializeField] private int minGateConnectivity = 3;
        [SerializeField] private float minGateDistance = 3f;
        [SerializeField] private TextMeshProUGUI playerSegmentText;
        
        #endregion

        private const int K_defaultMapSegmentSize = 10;
        
        [ShowInInspector, ReadOnly] private Vector2Int keyMapSegmentPosition;
        [ShowInInspector, ReadOnly] private Vector2Int exitMapSegmentPosition;
        [ShowInInspector, ReadOnly] private bool keyHasBeenSpawned;

        private void OnEnable()
        {
            SignalsHub.AddListener<MapItemRemovedEvent>(OnMapItemRemoved);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<MapItemRemovedEvent>(OnMapItemRemoved);
        }

        private void OnMapItemRemoved(MapItemRemovedEvent evt)
        {
            var itemPosition = ConvertWorldPositionToSegmentPosition(evt.GameObject.transform.position);
            if (positionsToMapSegmentKeys.TryGetValue(itemPosition, out var itemSegmentKey) && 
                mapSegmentKeysToMapSegments.TryGetValue(itemSegmentKey, out var itemSegment))
            {
                Debug.Log($"OnMapItemRemoved > remove item {evt.ItemType} from segment {itemSegmentKey}");
                itemSegment.ItemObjects.Remove(evt.GameObject);
            }
            
            Destroy(evt.GameObject);
        }

        private void Awake()
        {
            if (mapSegmentSize.x <= 0) mapSegmentSize.x = K_defaultMapSegmentSize;
            if (mapSegmentSize.y <= 0) mapSegmentSize.y = K_defaultMapSegmentSize;
        }

        private void Start()
        {
            var minDistanceBetweenKeyAndExit = Mathf.Max(halfMapSize.x / 2, halfMapSize.y / 2);
            keyMapSegmentPosition = GetRandomDistantMapSegmentPosition();
            exitMapSegmentPosition = GetRandomDistantMapSegmentPosition();
            while (Vector2Int.Distance(keyMapSegmentPosition, exitMapSegmentPosition) < minDistanceBetweenKeyAndExit)
            {
                exitMapSegmentPosition = GetRandomDistantMapSegmentPosition();
            }
        }

        private void Update()
        {
            var maybePlayer = mapActorRegistry.Player;
            maybePlayer.IfPresent(player =>
            {
                var playerIntPosition = ConvertWorldPositionToSegmentPosition(player.transform.position);
                
                var adjacentSegmentPositions = GetAdjacentSegmentPositions(playerIntPosition);
                adjacentSegments = adjacentSegmentPositions;
                if (playerSegmentText != null) playerSegmentText.text = $"P: {playerIntPosition}, S: {positionsToMapSegmentKeys[playerIntPosition]}";

                ShowAdjacentSegments(adjacentSegmentPositions);
                HideNonAdjacentSegments(adjacentSegmentPositions);
            });
        }

        private Vector2Int ConvertWorldPositionToSegmentPosition(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / mapSegmentSize.x),
                Mathf.FloorToInt(worldPosition.y / mapSegmentSize.y)
            );
        }

        private Vector2Int GetRandomDistantMapSegmentPosition()
        {
            var segmentPositionX = randomService.Int(-halfMapSize.x, halfMapSize.x);
            var segmentPositionY = randomService.Int(-halfMapSize.y, halfMapSize.y);

            return new Vector2Int(segmentPositionX, segmentPositionY);
        }
        
        private MapSegment GetMapSegment(Vector2Int segmentPosition)
        {
            if (!positionsToMapSegmentKeys.TryGetValue(segmentPosition, out var segmentKey))
            {
                return CreateMapSegment(segmentPosition);
            }

            if (mapSegmentKeysToMapSegments.TryGetValue(segmentKey, out var mapSegment))
            {
                if (segmentPosition == exitMapSegmentPosition && !mapSegment.ContainsMapObject<MapExit>())
                {
                    Debug.Log($"Segment {mapSegment} does not have a MapExit, adding one!");
                    var exit = GenerateExit(segmentPosition * mapSegmentSize, segmentPosition * mapSegmentSize + mapSegmentSize);
                    mapSegment.MapObjects.Add(exit);
                }
                
                if (!keyHasBeenSpawned && segmentPosition == keyMapSegmentPosition && !mapSegment.ContainsItem(ItemType.Key))
                {
                    Debug.Log($"Segment {mapSegment} does not have a Key item, adding one!");
                    var key = GenerateItem(ItemType.Key, segmentPosition * mapSegmentSize,
                        segmentPosition * mapSegmentSize + mapSegmentSize);
                    mapSegment.ItemObjects.Add(key);
                    keyHasBeenSpawned = true;
                }
                
                return mapSegment;
            }

            Debug.LogError(
                $"Data Inconsistency: Key {segmentKey} for position {segmentPosition} exists but the MapSegment does not!");

            return CreateMapSegment(segmentPosition);
        }

        private MapSegmentNeighbourToKeyDictionary FindAdjacentSegments(
            Vector2Int segmentPosition, out List<MapSegmentNeighbour> openNeighbourKeys)
        {
            openNeighbourKeys = new List<MapSegmentNeighbour>();
            var neighbours = new MapSegmentNeighbourToKeyDictionary();
            foreach (var neighbour in EnumExtensions.GetAllItems<MapSegmentNeighbour>())
            {
                var neighbourSegmentPosition = segmentPosition + neighbour.GetPosition();
                if (positionsToMapSegmentKeys.TryGetValue(neighbourSegmentPosition, out var neighbourKey))
                    neighbours.Add(neighbour, neighbourKey);
                else if (neighbour != MapSegmentNeighbour.None)
                    openNeighbourKeys.Add(neighbour);
            }

            return neighbours;
        }

        private IMaybe<MapSegment> FindGateSegment(Vector2Int segmentPosition, MapSegmentNeighbour neighbour)
        {
            var openSegments = mapSegmentKeysToMapSegments.Values
                .Where(segment => !segment.MapSegmentNeighbourKeys.ContainsKey(neighbour) &&
                                  GetShortestDistance(segmentPosition, segment.Key) >= minGateDistance).ToList();
            if (openSegments.None()) return Maybe.Empty<MapSegment>();

            return randomService.Sample(openSegments).ToMaybe();
        }

        private float GetShortestDistance(Vector2Int segmentPosition, string segmentKey)
        {
            return positionsToMapSegmentKeys
                .Where(pair => pair.Value == segmentKey).Select(pair => pair.Key)
                .Min(position => Vector2Int.Distance(segmentPosition, position));
        }

        private List<Vector2Int> GetAdjacentSegmentPositions(Vector2Int segmentPosition)
        {
            var mapSegment = GetMapSegment(segmentPosition);
            return EnumExtensions.GetAllItems<MapSegmentNeighbour>()
                .Select(neighbour =>
                {
                    var neighbourSegmentPosition = segmentPosition + neighbour.GetPosition();
                    if (mapSegment.MapSegmentNeighbourKeys.TryGetValue(neighbour, out var neighbourKey))
                    {
                        if (!positionsToMapSegmentKeys.ContainsKey(neighbourSegmentPosition))
                            positionsToMapSegmentKeys.Add(neighbourSegmentPosition, neighbourKey);

                        if (positionsToMapSegmentKeys[neighbourSegmentPosition] != neighbourKey)
                        {
                            mapSegment.MapSegmentNeighbourKeys[neighbour] =
                                positionsToMapSegmentKeys[neighbourSegmentPosition];
                        }

                        return neighbourSegmentPosition;
                    }

                    var neighbourMapSegment = GetMapSegment(neighbourSegmentPosition);
                    mapSegment.MapSegmentNeighbourKeys.Add(neighbour, neighbourMapSegment.Key);

                    return neighbourSegmentPosition;
                }).ToList();
        }

        private MapSegment CreateMapSegment(Vector2Int segmentPosition)
        {
            var topLeftCorner = segmentPosition * mapSegmentSize;
            var bottomRightCorner = topLeftCorner + mapSegmentSize;
            var mapObjects = new List<GameObject>();
            mapObjects.AddRange(GenerateTrees(topLeftCorner, bottomRightCorner));
            mapObjects.AddRange(GenerateGrass(topLeftCorner, bottomRightCorner));
            if (segmentPosition == exitMapSegmentPosition)
            {
                Debug.Log($"Generate Exit at {segmentPosition}!");
                mapObjects.Add(GenerateExit(topLeftCorner, bottomRightCorner));
            }

            var itemObjects = new List<GameObject>();
            if (!keyHasBeenSpawned && segmentPosition == keyMapSegmentPosition)
            {
                Debug.Log($"Generate Key at {segmentPosition}");
                itemObjects.Add(GenerateItem(ItemType.Key, topLeftCorner, bottomRightCorner));
                keyHasBeenSpawned = true;
            }

            var mapSegmentKey = MapSegment.ToMapSegmentCoordinatesKey(segmentPosition);
            var mapSegmentNeighboursKeys = FindAdjacentSegments(segmentPosition, out var openNeighbourKeys);

            if (openNeighbourKeys.Count >= minGateConnectivity)
            {
                var openNeighbour = openNeighbourKeys[randomService.Int(0, openNeighbourKeys.Count)];
                var oppositeNeighbour = openNeighbour.GetOpposite();
                var maybeGateSegment = FindGateSegment(segmentPosition, oppositeNeighbour);
                maybeGateSegment.IfPresent(gateSegment =>
                {
                    Debug.Log($"{nameof(CreateMapSegment)} > {openNeighbour}-{oppositeNeighbour} gate in {segmentPosition}={gateSegment.Position}");
                    mapSegmentNeighboursKeys.Add(openNeighbour, gateSegment.Key);
                    positionsToMapSegmentKeys.Add(segmentPosition + openNeighbour.GetPosition(), gateSegment.Key);
                });
            }

            var mapSegment = new MapSegment
            {
                Key = mapSegmentKey,
                Position = segmentPosition,
                MapSegmentNeighbourKeys = mapSegmentNeighboursKeys,
                MapObjects = mapObjects,
                ItemObjects = itemObjects,
            };

            positionsToMapSegmentKeys.Add(segmentPosition, mapSegment.Key);
            mapSegmentKeysToMapSegments.Add(mapSegment.Key, mapSegment);
            hiddenMapSegmentKeys.Add(mapSegment.Key);
            mapSegment.Hide();

            return mapSegment;
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

        private GameObject GenerateExit(Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
        {
            var cornerDiff = bottomRightCorner - topLeftCorner;
            var exitMapObject = mapObjectRegistry.GetMapObject(MapObjectType.Exit);
            var exitPosition = new Vector2(topLeftCorner.x + randomService.Int(cornerDiff.x), topLeftCorner.y + randomService.Int(cornerDiff.y));

            var exit = prefabPool.Spawn(exitMapObject.Prefab, grassParent);
            exit.GetComponent<MapObjectController>().SetMapObject(exitMapObject);
            exit.transform.position = exitPosition;

            return exit;
        }
        
        private GameObject GenerateItem(ItemType key, Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
        {
            var cornerDiff = bottomRightCorner - topLeftCorner;
            var itemObject = itemRegistry.GetItem(ItemType.Key);
            var itemPosition = new Vector2(topLeftCorner.x + randomService.Int(cornerDiff.x), topLeftCorner.y + randomService.Int(cornerDiff.y));

            var item = prefabPool.Spawn(itemObject.ItemPrefab, grassParent);
            item.GetComponent<ItemController>().SetItem(itemObject);
            item.transform.position = itemPosition;

            return item;
        }
        
        private void ShowAdjacentSegments(List<Vector2Int> adjacentSegmentPositions)
        {
            foreach (var position in adjacentSegmentPositions)
            {
                var segmentKey = positionsToMapSegmentKeys[position];
                var mapSegment = mapSegmentKeysToMapSegments[segmentKey];
                if (hiddenMapSegmentKeys.Contains(segmentKey))
                {
                    hiddenMapSegmentKeys.Remove(segmentKey);
                    shownMapSegmentKeys.Add(segmentKey);
                    mapSegment.Move(position, mapSegmentSize);
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
                .Select(position => positionsToMapSegmentKeys.TryGetValue(position, out var key) ? key : string.Empty)
                .ToList();
            var keysToRemove = new List<string>();
            foreach (var key in shownMapSegmentKeys)
            {
                if (adjacentSegmentKeys.Contains(key)) continue;
                keysToRemove.Add(key);
                hiddenMapSegmentKeys.Add(key);
                var mapSegment = mapSegmentKeysToMapSegments[key];
                mapSegment.Hide();
            }

            foreach (var key in keysToRemove)
            {
                shownMapSegmentKeys.Remove(key);
            }
        }
    }

    [Serializable]
    public class Vector2IntToStringDictionary : UnitySerializedDictionary<Vector2Int, string>
    {
    }

    [Serializable]
    public class StringToMapSegmentDictionary : UnitySerializedDictionary<string, MapSegment>
    {
    }
}