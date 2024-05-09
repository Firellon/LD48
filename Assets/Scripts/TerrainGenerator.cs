using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Day;
using LD48;
using Map;
using TMPro;
using UnityEngine;
using Utilities.Prefabs;
using Random = UnityEngine.Random;
using Zenject;

public class TerrainGenerator : MonoBehaviour
{
    [Inject] private IMapActorRegistry mapActorRegistry;
    [Inject] private IPrefabPool prefabPool;

    private DayNightCycle dayNightCycle;
    public Vector2Int levelSize = new(10, 10);

    #region Items

    public GameObject woodPrefab;
    public Transform itemParent;
    public Vector2Int itemDensity = new Vector2Int(2, 2);
    public float itemSpawnProbability = 0.1f;

    private List<Vector2> itemPositions = new List<Vector2>();
    private readonly List<GameObject> items = new List<GameObject>();

    #endregion

    #region Strangers

    public List<GameObject> strangerPrefabs;
    public Transform strangerParent;
    public Vector2Int strangerDensity = new Vector2Int(5, 5);
    public float strangerSpawnProbability = 0.2f;

    private readonly List<GameObject> strangers = new List<GameObject>();

    #endregion

    #region Player

    public GameObject playerPrefab;
    private GameObject player;
    private CinemachineVirtualCamera cinemachineCam;
    private GameObject playerObject;
    public List<ObjectFollower> playerFollowers;
    public TMP_Text tipMessageText;
    public TMP_Text woodAmountText;

    #endregion

    #region Dead

    public GameObject deadPrefab;
    public Vector2 deadDensity = new Vector2Int(25, 25);
    public Transform deadParent;
    public float deadSpawnProbability = 0.25f;

    private List<Transform> deads = new List<Transform>();

    #endregion

    #region Ghosts

    public GameObject ghostPrefab;
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

        // GenerateTrees();
        GenerateItems(itemSpawnProbability);
        GenerateStrangers(strangerSpawnProbability);
        GenerateDead();
        GeneratePlayer();
    }

    // Update is called once per frame
    void Update()
    {
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
        itemPositions = new List<Vector2>();
        for (var itemX = itemDensity.x / 2; itemX < levelSize.x; itemX += itemDensity.x)
        {
            for (var itemY = itemDensity.y / 2; itemY < levelSize.y; itemY += itemDensity.y)
            {
                if (Random.value > spawnProbability) continue;
                var itemPosition = new Vector2(itemX + Random.Range(0f, itemDensity.x),
                    itemY + Random.Range(0f, itemDensity.y));
                itemPositions.Add(itemPosition);
                var wood = prefabPool.Spawn(woodPrefab, itemParent);
                wood.transform.position += new Vector3(itemPosition.x, itemPosition.y, 0);
                items.Add(wood);
            }
        }
    }

    public void GenerateStrangers(float spawnProbability)
    {
        for (var strangerX = strangerDensity.x / 2; strangerX < levelSize.x; strangerX += strangerDensity.x)
        {
            for (var strangerY = strangerDensity.y / 2; strangerY < levelSize.y; strangerY += strangerDensity.y)
            {
                if (Random.value > spawnProbability) continue;
                var strangerPosition = new Vector2(strangerX + Random.Range(0f, strangerDensity.x),
                    strangerY + Random.Range(0f, strangerDensity.y));
                var stranger = prefabPool.Spawn(strangerPrefabs[Random.Range(0, strangerPrefabs.Count)], strangerParent);
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
        playerObject = prefabPool.Spawn(playerPrefab, new Vector2(levelSize.x / 2, levelSize.y / 2), Quaternion.identity);
        var player = playerObject.GetComponent<Player>();
        player.tipMessageText = tipMessageText;
        player.woodAmountText = woodAmountText;
        
        cinemachineCam.Follow = player.transform;

        foreach (var playerFollower in playerFollowers)
        {
            playerFollower.SetTarget(playerObject.transform);
        }

        mapActorRegistry.SetPlayer(player);
    }

    public void GenerateGhosts()
    {
        foreach (var dead in deads)
        {
            if (Random.value > ghostSpawnProbability + dayNightCycle.GetCurrentDay() * 0.1f) continue;
            var ghost = prefabPool.Spawn(ghostPrefab, dead.position, dead.rotation);
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