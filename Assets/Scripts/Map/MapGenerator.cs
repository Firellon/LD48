using System;
using System.Collections.Generic;
using System.Linq;
using Environment;
using Inventory;
using Inventory.Signals;
using LD48;
using Map.Actor;
using Plugins.Sirenix.Odin_Inspector.Modules;
using Signals;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;
using Utilities.Monads;
using Utilities.Prefabs;
using Utilities.RandomService;
using Zenject;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        [Inject] private IMapActorRegistry mapActorRegistry;
        [Inject] private IPrefabPool prefabPool;
        [Inject] private IRandomService randomService;
        [Inject] private IMapObjectRegistry mapObjectRegistry;
        [Inject] private IItemRegistry itemRegistry;

        [SerializeField] private int totalDiaryCount = 30; // TODO: Implement the actual count based on the amount of written Diary texts
        [SerializeField] private Vector2Int halfMapSize = new(20, 20);
        [SerializeField] private Vector2Int mapSegmentSize = new(10, 10);

        [ShowInInspector, ReadOnly] private StringToMapSegmentDictionary mapSegmentKeysToMapSegments = new();
        [ShowInInspector, ReadOnly] private Vector2IntToStringDictionary positionsToMapSegmentKeys = new();
        [ShowInInspector, ReadOnly] private HashSet<string> shownMapSegmentKeys = new();
        [ShowInInspector, ReadOnly] private HashSet<string> hiddenMapSegmentKeys = new();
        [ShowInInspector, ReadOnly] private List<Vector2Int> adjacentSegments = new(); // TODO: Remove

        #region Trails

        [Space(10)]
        [SerializeField] private GameObject trailsTilemapPrefab;
        [SerializeField] private RuleTile trailRuleTile;

        private GameObject testMaze;

        [Range(0f, 10f)] [SerializeField] private float trailNoiseScale = 0.2f;
        [Range(0f, 1f)] [SerializeField] private float trailNoiseBorder = 0.37f;

        private float prevScale = 0.5f;
        private float prevBorder = 0.4f;

        private MazeGenerator mazeGen;

        #endregion

        #region Trees
        [Space(10)] 
        [SerializeField] private Transform treeParent;
        [SerializeField] private Vector2Int treeDensity = new(2, 2);
        [SerializeField] private float treeSpawnProbability = 0.5f;

        #endregion

        #region Grass

        [SerializeField] private Transform grassParent;
        [SerializeField] private Vector2Int grassDensity = new(2, 2);
        [SerializeField] private float grassSpawnProbability = 0.5f;

        #endregion

        #region MapObjects

        [Space(10)] 
        [SerializeField] private Transform mapObjectParent;
        [SerializeField] private Vector2Int mapObjectDensity = new(4, 4);
        [SerializeField] private float mapObjectSpawnProbability = 0.1f;
        [SerializeField] private List<MapObjectType> spawnMapObjectTypes = new();
        
        #endregion

        #region Items

        [SerializeField] private Transform itemParent;
        [SerializeField] private Vector2Int itemDensity = new(5, 5);
        [SerializeField] private float itemSpawnProbability = 0.1f;
        [SerializeField] private List<ItemType> spawnItemTypes = new();

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
        [ShowInInspector, ReadOnly] private List<Vector2Int> diaryMapSegmentPositions;
        [ShowInInspector, ReadOnly] private bool keyHasBeenSpawned;

        private void OnEnable()
        {
            SignalsHub.AddListener<MapObjectAddedEvent>(OnMapObjectAdded);
            SignalsHub.AddListener<MapItemRemovedEvent>(OnMapItemRemoved);
            SignalsHub.AddListener<MapObjectRemovedEvent>(OnMapObjectRemoved);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<MapObjectAddedEvent>(OnMapObjectAdded);
            SignalsHub.RemoveListener<MapItemRemovedEvent>(OnMapItemRemoved);
            SignalsHub.RemoveListener<MapObjectRemovedEvent>(OnMapObjectRemoved);
        }

        private void OnMapObjectAdded(MapObjectAddedEvent evt)
        {
            var mapObjectPosition = ConvertWorldPositionToSegmentPosition(evt.GameObject.transform.position);
            if (positionsToMapSegmentKeys.TryGetValue(mapObjectPosition, out var mapObjectSegmentKey) &&
                mapSegmentKeysToMapSegments.TryGetValue(mapObjectSegmentKey, out var mapSegment))
            {
                Debug.Log($"{nameof(OnMapObjectAdded)} > add map object {evt.MapObjectType} to segment {mapObjectSegmentKey}");
                mapSegment.MapObjects.Add(evt.GameObject);
            }
        }

        private void OnMapItemRemoved(MapItemRemovedEvent evt)
        {
            var itemPosition = ConvertWorldPositionToSegmentPosition(evt.GameObject.transform.position);
            if (positionsToMapSegmentKeys.TryGetValue(itemPosition, out var itemSegmentKey) &&
                mapSegmentKeysToMapSegments.TryGetValue(itemSegmentKey, out var itemSegment))
            {
                Debug.Log($"{nameof(OnMapItemRemoved)} > remove item {evt.ItemType} from segment {itemSegmentKey}");
                itemSegment.ItemObjects.Remove(evt.GameObject);
            }

            Destroy(evt.GameObject);
        }
        
        private void OnMapObjectRemoved(MapObjectRemovedEvent evt)
        {
            var mapObjectPosition = ConvertWorldPositionToSegmentPosition(evt.GameObject.transform.position);
            if (positionsToMapSegmentKeys.TryGetValue(mapObjectPosition, out var mapObjectSegmentKey) &&
                mapSegmentKeysToMapSegments.TryGetValue(mapObjectSegmentKey, out var mapObjectSegment))
            {
                Debug.Log($"{nameof(OnMapObjectRemoved)} > remove map object {evt.ObjectType} from segment {mapObjectSegmentKey}");
                mapObjectSegment.MapObjects.Remove(evt.GameObject);
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
            diaryMapSegmentPositions = GetRandomMapSegmentPositions(totalDiaryCount);
            while (Vector2Int.Distance(keyMapSegmentPosition, exitMapSegmentPosition) < minDistanceBetweenKeyAndExit)
            {
                exitMapSegmentPosition = GetRandomDistantMapSegmentPosition();
            }
        }

        private void RegenerateMaze()
        {
            if (testMaze != null)
            {
                testMaze.GetComponentInChildren<Tilemap>().ClearAllTiles();
                prefabPool.Despawn(testMaze);
                testMaze = null;
            }

            var position = new Vector3(50f, 50f);
            var trailsTilemap = prefabPool.Spawn(trailsTilemapPrefab).GetComponentInChildren<Tilemap>();
            trailsTilemap.transform.position = position;

            if (mazeGen == null)
            {
                mazeGen = new MazeGenerator(20, 20);
                mazeGen.GenerateNewRandomMaze();
            }

            for (int x = 0; x < mazeGen.MazeWidth; x++)
            {
                for (int y = 0; y < mazeGen.MazeHeight; y++)
                {
                    if (!mazeGen.MazeGrid[x, y])
                    {
                        var noiseVal = GetPerlinValue(new Vector3Int(x, y, 0), trailNoiseScale, 0f);
                        if (noiseVal > trailNoiseBorder)
                        {
                            trailsTilemap.SetTile(new Vector3Int(x, y, 0), trailRuleTile);
                        }
                    }
                }
            }

            testMaze = trailsTilemap.gameObject;
        }

        private void Update()
        {
            // if (Mathf.Abs(noiseScale - prevScale) > Mathf.Epsilon || Mathf.Abs(noiseBorder - prevBorder) > Mathf.Epsilon)
            // {
            //     RegenerateMaze();
            //     prevScale = noiseScale;
            //     prevBorder = noiseBorder;
            // }

            var maybePlayer = mapActorRegistry.Player;
            maybePlayer.IfPresent(player =>
            {
                var playerIntPosition = ConvertWorldPositionToSegmentPosition(player.transform.position);

                var adjacentSegmentPositions = GetAdjacentSegmentPositions(playerIntPosition);
                adjacentSegments = adjacentSegmentPositions;
                if (playerSegmentText != null)
                    playerSegmentText.text =
                        $"P: {playerIntPosition}, S: {positionsToMapSegmentKeys[playerIntPosition]}";

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

        private Vector2 ConvertSegmentPositionToWorldPosition(Vector2Int segmentPosition)
        {
            return segmentPosition * mapSegmentSize + ((Vector2)mapSegmentSize) / 2f;
        }

        private Vector2Int GetRandomDistantMapSegmentPosition()
        {
            var segmentPositionX = randomService.Int(-halfMapSize.x, halfMapSize.x);
            var segmentPositionY = randomService.Int(-halfMapSize.y, halfMapSize.y);

            return new Vector2Int(segmentPositionX, segmentPositionY);
        }

        private List<Vector2Int> GetRandomMapSegmentPositions(int positionCount)
        {
            var mapPositions = new List<Vector2Int>();
            while (mapPositions.Count < positionCount)
            {
                var segmentPositionX = randomService.Int(-halfMapSize.x, halfMapSize.x);
                var segmentPositionY = randomService.Int(-halfMapSize.y, halfMapSize.y);
                var newPosition = new Vector2Int(segmentPositionX, segmentPositionY);
                
                if (mapPositions.Any(position => position.x == newPosition.x && position.y == newPosition.y)) continue;
                mapPositions.Add(newPosition);    
            }

            return mapPositions;
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
                    var exit = GenerateExit(segmentPosition * mapSegmentSize,
                        segmentPosition * mapSegmentSize + mapSegmentSize);
                    mapSegment.MapObjects.Add(exit);
                }

                if (!keyHasBeenSpawned && segmentPosition == keyMapSegmentPosition &&
                    !mapSegment.ContainsItem(ItemType.Key))
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
            mapObjects.Add(GenerateTrails(segmentPosition));

            if (segmentPosition == exitMapSegmentPosition)
            {
                Debug.Log($"Generate Exit at {segmentPosition}!");
                mapObjects.Add(GenerateExit(topLeftCorner, bottomRightCorner));
            }
            mapObjects.AddRange(GenerateMapObjects(topLeftCorner, bottomRightCorner));

            var itemObjects = new List<GameObject>();
            itemObjects.AddRange(GenerateItems(topLeftCorner, bottomRightCorner));
            if (!keyHasBeenSpawned && segmentPosition == keyMapSegmentPosition)
            {
                Debug.Log($"Generate Key at {segmentPosition}");
                itemObjects.Add(GenerateItem(ItemType.Key, topLeftCorner, bottomRightCorner));
                keyHasBeenSpawned = true;
            }

            // if (diaryMapSegmentPositions.Contains(segmentPosition))
            // {
                Debug.Log($"Generate Diary at {segmentPosition}");
                mapObjects.Add(GenerateDiary(topLeftCorner, bottomRightCorner));
            // }

            var mapSegmentKey = MapSegment.ToMapSegmentCoordinatesKey(segmentPosition);
            var mapSegmentNeighboursKeys = FindAdjacentSegments(segmentPosition, out var openNeighbourKeys);

            if (openNeighbourKeys.Count >= minGateConnectivity)
            {
                var openNeighbour = openNeighbourKeys[randomService.Int(0, openNeighbourKeys.Count)];
                var oppositeNeighbour = openNeighbour.GetOpposite();
                var maybeGateSegment = FindGateSegment(segmentPosition, oppositeNeighbour);
                maybeGateSegment.IfPresent(gateSegment =>
                {
                    Debug.Log(
                        $"{nameof(CreateMapSegment)} > {openNeighbour}-{oppositeNeighbour} gate in {segmentPosition}={gateSegment.Position}");
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

        public List<GameObject> GenerateMapObjects(Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
        {
            var mapObjects = new List<GameObject>();
            if (spawnMapObjectTypes.None())
            {
                Debug.LogWarning($"{nameof(GenerateMapObjects)} > spawnMapObjectTypes is empty, no mapObjects to generate!");
                return mapObjects;
            }

            for (var mapObjectX = topLeftCorner.x; mapObjectX < bottomRightCorner.x; mapObjectX += mapObjectDensity.x)
            {
                for (var mapObjectY = topLeftCorner.y; mapObjectY < bottomRightCorner.y; mapObjectY += mapObjectDensity.y)
                {
                    if (randomService.Float() > mapObjectSpawnProbability) continue;
                    var baseMapObjectPosition = new Vector2(mapObjectX + randomService.Float(0f, mapObjectDensity.x),
                        mapObjectY + randomService.Float(0f, mapObjectDensity.y));
                    var mapObjectType = randomService.Sample(spawnMapObjectTypes);
                    var mapObject = mapObjectRegistry.GetMapObject(mapObjectType);
                    var mapObjectGO = prefabPool.Spawn(mapObject.Prefab, mapObjectParent);
                    mapObjectGO.GetComponent<MapObjectController>().SetMapObject(mapObject);
                    mapObjectGO.transform.position += new Vector3(baseMapObjectPosition.x + randomService.Float(-2, 2),
                        baseMapObjectPosition.y + randomService.Float(-2, 2), 0);
                    mapObjects.Add(mapObjectGO);
                    Debug.Log($"Added {mapObjectType} mapObject to the segment!");
                }
            }

            return mapObjects;
        }

        private List<GameObject> GenerateItems(Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
        {
            var itemObjects = new List<GameObject>();
            if (spawnItemTypes.None())
            {
                Debug.LogWarning("GenerateItems > spawnItemTypes is empty, no items to generate!");
                return itemObjects;
            }

            for (var itemX = topLeftCorner.x; itemX < bottomRightCorner.x; itemX += itemDensity.x)
            {
                for (var itemY = topLeftCorner.y; itemY < bottomRightCorner.y; itemY += itemDensity.y)
                {
                    if (randomService.Float() > itemSpawnProbability) continue;
                    var baseItemPosition = new Vector2(itemX + randomService.Float(0f, itemDensity.x),
                        itemY + randomService.Float(0f, itemDensity.y));
                    var itemType = randomService.Sample(spawnItemTypes);
                    var itemMapObject = itemRegistry.GetItem(itemType);
                    var item = prefabPool.Spawn(itemMapObject.ItemPrefab, itemParent);
                    item.GetComponent<ItemController>().SetItem(itemMapObject);
                    item.transform.position += new Vector3(baseItemPosition.x + randomService.Float(-2, 2),
                        baseItemPosition.y + randomService.Float(-2, 2), 0);
                    itemObjects.Add(item);
                    Debug.Log($"Added {itemType} item to the segment!");
                }
            }

            return itemObjects;
        }

        /// <summary>
        /// Returns a Perlin Noise value based on the given inputs.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="scale">The Perlin Scale factor of the Tile.</param>
        /// <param name="offset">Offset of the Tile on the Tilemap.</param>
        /// <returns>A Perlin Noise value based on the given inputs.</returns>
        public static float GetPerlinValue(Vector3Int position, float scale, float offset)
        {
            return Mathf.PerlinNoise((position.x + offset) * scale, (position.y + offset) * scale);
        }

        private GameObject GenerateTrails(Vector2Int segmentPosition)
        {
            var position = ConvertSegmentPositionToWorldPosition(segmentPosition);
            var trailsTilemap = prefabPool.Spawn(trailsTilemapPrefab).GetComponentInChildren<Tilemap>();
            trailsTilemap.transform.position = position;

            var mazeGen = new MazeGenerator(20, 20);
            mazeGen.GenerateNewRandomMaze();

            // for (int x = 0; x < mazeGen.MazeWidth; x++)
            // {
            //     // if (randomService.Chance(0.5f))
            //     //     continue;
            //
            //     for (int y = 0; y < mazeGen.MazeHeight; y++)
            //     {
            //         if ((x == 0) || (x == mazeGen.MazeHeight-1) || y == 0 || (y == mazeGen.MazeWidth-1)) // || y % 2 == 0)
            //             continue;
            //
            //         // if (randomService.Chance(0.5f))
            //         //     continue;
            //
            //         if (!mazeGen.MazeGrid[x, y])
            //         {
            //             var noiseVal = GetPerlinValue(new Vector3Int(x, y, 0), noiseScale, 100000f);
            //             if (noiseVal > noiseBorder)
            //             {
            //                 trailsTilemap.SetTile(new Vector3Int(x, y, 0), trailRuleTile);
            //             }
            //         }
            //     }
            // }

            var maze = new (bool Value, float Transparency)[20, 20];

            var width = maze.GetLength(0);
            var height = maze.GetLength(1);

            var canChangeDirection = true;

            var directionChangedIndex = -1;
            var directionChangedType = 0;

            var transparency = randomService.Float(0.3f, 0.6f);

            // generate 2 to 4 lines
            for (int i = 0; i < randomService.Int(2, 4); i++)
            {
                var index = randomService.Int(0, width);

                var startIndex = randomService.Int(0, Mathf.FloorToInt(width / 2f));
                var endIndex = randomService.Int(Mathf.FloorToInt(width / 2f), width);

                var isVertical = randomService.Chance(randomService.Float(0.5f, 0.7f));

                // X
                if (isVertical)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (j < startIndex || j > endIndex)
                            continue;

                        maze[index, j] = (true, transparency); //(float)j / (endIndex - startIndex));
                    }
                }
                // Y
                else
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (j < startIndex || j > endIndex)
                            continue;

                        maze[j, index] = (true, transparency); //(float)j / (endIndex - startIndex));
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // if ((x == 0) || (x == mazeGen.MazeHeight-1) || y == 0 || (y == mazeGen.MazeWidth-1)) // || y % 2 == 0)
                    //     continue;

                    // if (randomService.Chance(0.05f))
                    //     continue;

                    if (maze[x, y].Value)
                    {
                        var noiseVal = GetPerlinValue(new Vector3Int(x, y, 0), trailNoiseScale, 100000f);
                        if (noiseVal > trailNoiseBorder)
                        {
                            var mazePos = new Vector3Int(x, y, 0);
                            trailsTilemap.SetTile(mazePos, trailRuleTile);
                            trailsTilemap.SetColor(mazePos, new Color(1f, 1f, 1f, maze[x, y].Transparency));
                        }
                    }
                }
            }

            return trailsTilemap.gameObject;
        }

        private GameObject GenerateExit(Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
        {
            var cornerDiff = bottomRightCorner - topLeftCorner;
            var exitMapObject = mapObjectRegistry.GetMapObject(MapObjectType.Exit);
            var exitPosition = new Vector2(topLeftCorner.x + randomService.Int(cornerDiff.x),
                topLeftCorner.y + randomService.Int(cornerDiff.y));

            var exit = prefabPool.Spawn(exitMapObject.Prefab, mapObjectParent);
            exit.GetComponent<MapObjectController>().SetMapObject(exitMapObject);
            exit.transform.position = exitPosition;

            return exit;
        }

        private GameObject GenerateDiary(Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
        {
            var cornerDiff = bottomRightCorner - topLeftCorner;
            var diaryMapObject = mapObjectRegistry.GetMapObject(MapObjectType.Diary);
            var diaryPosition = new Vector2(topLeftCorner.x + randomService.Int(cornerDiff.x),
                topLeftCorner.y + randomService.Int(cornerDiff.y));

            var diary = prefabPool.Spawn(diaryMapObject.Prefab, mapObjectParent);
            diary.GetComponent<MapObjectController>().SetMapObject(diaryMapObject);
            diary.transform.position = diaryPosition;

            return diary;
        }

        private GameObject GenerateItem(ItemType key, Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
        {
            var cornerDiff = bottomRightCorner - topLeftCorner;
            var itemObject = itemRegistry.GetItem(ItemType.Key);
            var itemPosition = new Vector2(topLeftCorner.x + randomService.Int(cornerDiff.x),
                topLeftCorner.y + randomService.Int(cornerDiff.y));

            var item = prefabPool.Spawn(itemObject.ItemPrefab, itemParent);
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