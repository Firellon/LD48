using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Day;
using Human;
using Inventory;
using LD48;
using Map.Actor;
using Player;
using TMPro;
using UnityEngine;
using Utilities.Monads;
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

    private List<Transform> deads = new();

    #endregion

    #region Ghosts

    public Transform ghostParent;
    [SerializeField] private int baseGhostSpawnProbability = 4;
    private List<GameObject> ghosts = new();

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

        // GenerateItems(itemSpawnProbability);
        GenerateStrangers(strangerSpawnProbability);
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
        var playerState = mapActorRegistry.Player.Match(player => player.State.ToMaybe(), Maybe.Empty<HumanState>());
        var playerSanity = playerState.Match(state => (float) state.Sanity / (state.MaxSanity - state.MinSanity), 1f);
        var ghostMapActor = mapActorRegistry.GetMapActor(MapActorType.Ghost);
        var ghostSpawnProbability = (1 - playerSanity) * (float) dayNightCycle.GetCurrentDay() / (baseGhostSpawnProbability + dayNightCycle.GetCurrentDay());
        foreach (var dead in deads)
        {
            if (Random.value > ghostSpawnProbability) continue;
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