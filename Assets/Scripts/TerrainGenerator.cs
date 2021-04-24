using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Vector2Int levelSize = new Vector2Int(10, 10);

    #region Trees
    public GameObject treePrefab;
    public Transform treeParent;
    public Vector2Int treeDensity = new Vector2Int(2, 2);
    public float treeSpawnProbability = 0.5f;
    
    private List<Vector2> treePositions = new List<Vector2>();
    private readonly List<GameObject> trees = new List<GameObject>();
    #endregion

    #region Items
    public GameObject woodPrefab;
    public Transform itemParent;
    public Vector2Int itemDensity = new Vector2Int(2, 2);
    public float itemSpawnProbability = 0.1f;
    
    private List<Vector2> itemPositions = new List<Vector2>();
    private readonly List<GameObject> items = new List<GameObject>();
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        GenerateTrees();
        GenerateItems();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DeleteTrees()
    {
        foreach (var tree in trees)
        {
            Destroy(tree);
        }
        treePositions.Clear();
    }
    
    void DeleteItems()
    {
        foreach (var item in items)
        {
            Destroy(item);
        }
        itemPositions.Clear();
    }

    void GenerateTrees()
    {
        treePositions.Clear();
        for (var treeX = 0; treeX < levelSize.x; treeX += treeDensity.x)
        {
            for (var treeY = 0; treeY < levelSize.y; treeY += treeDensity.y)
            {
                if (Random.value > treeSpawnProbability)
                {
                    var treePosition = new Vector2(treeX + Random.Range(0f, treeDensity.x), treeY + Random.Range(0f, treeDensity.y));
                    treePositions.Add(treePosition);
                    var tree = Instantiate(treePrefab, treeParent);
                    tree.transform.position += new Vector3(treePosition.x, treePosition.y, 0);
                    trees.Add(tree);
                }
            }
        }
    }
    
    void GenerateItems()
    {
        itemPositions = new List<Vector2>();
        for (var itemX = itemDensity.x / 2; itemX < levelSize.x; itemX += itemDensity.x)
        {
            for (var itemY = itemDensity.y / 2; itemY < levelSize.y; itemY += itemDensity.y)
            {
                if (Random.value > itemSpawnProbability)
                {
                    var itemPosition = new Vector2(itemX + Random.Range(0f, itemDensity.x), itemY + Random.Range(0f, itemDensity.y));
                    treePositions.Add(itemPosition);
                    var wood = Instantiate(woodPrefab, itemParent);
                    wood.transform.position += new Vector3(itemPosition.x, itemPosition.y, 0);
                    items.Add(wood);
                }
            }
        }
    }
}
