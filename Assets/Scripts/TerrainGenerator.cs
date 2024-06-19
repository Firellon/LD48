using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Day;
using Inventory;
using LD48;
using Map.Actor;
using Player;
using TMPro;
using UnityEngine;
using Utilities.Prefabs;
using Utilities.RandomService;
using Random = UnityEngine.Random;
using Zenject;

public class TerrainGenerator : MonoBehaviour
{
    [Inject] private IMapActorRegistry mapActorRegistry;
    [Inject] private IPrefabPool prefabPool;
    [Inject] private IItemRegistry itemRegistry;
    [Inject] private IRandomService randomService;

    private DayNightCycle dayNightCycle;
    public Vector2Int levelSize = new(10, 10);

    #region MapObjects

    public Transform itemParent;
    public Vector2Int itemDensity = new(2, 2);
    public float itemSpawnProbability = 0.1f;
    [SerializeField] private ItemTypeToIntSerializedDictionary itemTypeToSpawnProbabilityMap = new();
    private List<ItemType> itemSpawnSet = new();

    private List<Vector2> itemPositions = new();
    private readonly List<GameObject> items = new();

    #endregion

    #region Strangers

    public Transform strangerParent;
    public Vector2Int strangerDensity = new(5, 5);
    public float strangerSpawnProbability = 0.2f;

    private readonly List<GameObject> strangers = new();

    #endregion

    #region PlayerController

    private GameObject player;
    private CinemachineVirtualCamera cinemachineCam;
    public List<ObjectFollower> playerFollowers;
    public TMP_Text tipMessageText;

    #endregion

    #region Dead

    public GameObject deadPrefab;
    public Vector2 deadDensity = new Vector2Int(25, 25);
    public Transform deadParent;
    public float deadSpawnProbability = 0.25f;

    private List<Transform> deads = new List<Transform>();

    #endregion

    #region Ghosts

    public Transform ghostParent;
    public float ghostSpawnProbability = 0.2f;
    private List<GameObject> ghosts = new List<GameObject>();

    #endregion

    private void Awake()
    {
        cinemachineCam = GetComponent<CinemachineVirtualCamera>();
    }


    // Start is called before the first frame update
    void Start()
    {
        dayNightCycle = GetComponent<DayNightCycle>();
        itemSpawnSet = itemTypeToSpawnProbabilityMap.SelectMany(pair =>
            Enumerable.Range(1, pair.Value).Select(_ => pair.Key).Take(pair.Value)).ToList();

        GenerateItems(itemSpawnProbability);
        GenerateStrangers(strangerSpawnProbability);
        GenerateDead();
        GeneratePlayer();
    }

    private void DeleteItems()
    {
        foreach (var item in items)
        {
            Destroy(item);
        }

        itemPositions.Clear();
    }

    public void GenerateItems(float spawnProbability)
    {
        DeleteItems();

        if (itemSpawnSet.None())
        {
            Debug.LogWarning("GenerateItems > itemSpawnSet is empty, no items to generate!");
            return;
        }

        for (var itemX = itemDensity.x / 2; itemX < levelSize.x; itemX += itemDensity.x)
        {
            for (var itemY = itemDensity.y / 2; itemY < levelSize.y; itemY += itemDensity.y)
            {
                if (Random.value > spawnProbability) continue;
                var itemPosition = new Vector2(itemX + Random.Range(0f, itemDensity.x),
                    itemY + Random.Range(0f, itemDensity.y));
                itemPositions.Add(itemPosition);
                var itemType = randomService.Sample(itemSpawnSet);
                var itemPrefab = itemRegistry.GetItem(itemType).ItemPrefab;
                var itemObject = prefabPool.Spawn(itemPrefab, itemParent);
                itemObject.transform.position += new Vector3(itemPosition.x, itemPosition.y, 0);
                items.Add(itemObject);
            }
        }
    }

    public void GenerateStrangers(float spawnProbability)
    {
        var strangerMapActor = mapActorRegistry.GetMapActor(MapActorType.Stranger);
        for (var strangerX = strangerDensity.x / 2; strangerX < levelSize.x; strangerX += strangerDensity.x)
        {
            for (var strangerY = strangerDensity.y / 2; strangerY < levelSize.y; strangerY += strangerDensity.y)
            {
                if (Random.value > spawnProbability) continue;
                var strangerPosition = new Vector2(strangerX + Random.Range(0f, strangerDensity.x),
                    strangerY + Random.Range(0f, strangerDensity.y));
                var strangerPrefab = strangerMapActor.GetRandomPrefab();
                var stranger = prefabPool.Spawn(strangerPrefab, strangerParent);
                stranger.transform.position += new Vector3(strangerPosition.x, strangerPosition.y, 0);
                strangers.Add(stranger);
            }
        }
    }

    private void GenerateDead()
    {
        for (var deadX = deadDensity.x / 2; deadX < levelSize.x; deadX += deadDensity.x)
        {
            for (var deadY = deadDensity.y / 2; deadY < levelSize.y; deadY += deadDensity.y)
            {
                if (Random.value > deadSpawnProbability) continue;
                var deadPosition = new Vector2(deadX + Random.Range(0f, deadDensity.x),
                    deadY + Random.Range(0f, deadDensity.y));
                var dead = prefabPool.Spawn(deadPrefab, deadParent);
                dead.transform.position += new Vector3(deadPosition.x, deadPosition.y, 0);
                deads.Add(dead.transform);
            }
        }
    }

    private void GeneratePlayer()
    {
        var playerMapActor = mapActorRegistry.GetMapActor(MapActorType.Player);
        var playerObject = prefabPool.Spawn(playerMapActor.Prefab, new Vector2(levelSize.x / 2, levelSize.y / 2),
            Quaternion.identity);
        var playerController = playerObject.GetComponent<PlayerController>();
        playerController.tipMessageText = tipMessageText;

        cinemachineCam.Follow = playerController.transform;

        foreach (var playerFollower in playerFollowers)
        {
            playerFollower.SetTarget(playerObject.transform);
        }

        mapActorRegistry.SetPlayer(playerController);
    }

    public void GenerateGhosts()
    {
        var ghostMapActor = mapActorRegistry.GetMapActor(MapActorType.Ghost);
        foreach (var dead in deads)
        {
            if (Random.value > ghostSpawnProbability + dayNightCycle.GetCurrentDay() * 0.1f) continue;
            var ghost = prefabPool.Spawn(ghostMapActor.Prefab, dead.position, dead.rotation);
            ghost.transform.parent = ghostParent;
            ghosts.Add(ghost);
        }
    }

    public void DestroyGhosts()
    {
        foreach (var ghost in ghosts.Where(ghost => ghost != null))
        {
            ghost.GetComponent<Ghost>().Hit();
        }

        ghosts.Clear();
    }

    public void AddDead(Transform newDead)
    {
        deads.Add(newDead);
    }
}