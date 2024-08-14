using System;
using System.Collections.Generic;
using Inventory;
using LD48;
using Plugins.Sirenix.Odin_Inspector.Modules;
using UnityEngine;
using UnityEngine.Rendering;

namespace Stranger.AI
{
    [CreateAssetMenu(menuName = "LD48/Create StrangerAiConfig", fileName = "StrangerAiConfig", order = 0)]
    public class StrangerAiConfig : ScriptableObject
    {
        [SerializeField] private StrangerState defaultState = StrangerState.Wander;
        public StrangerState DefaultState => defaultState;
        
        [Header("Threats")]
        [SerializeField] private float threatRadius = 10f;
        [SerializeField] private LayerMask threatLayerMask;
        [SerializeField] private int bravery = 2;
        
        public float ThreatRadius => threatRadius;
        public int ThreatLayerMask => threatLayerMask;
        public int Bravery => bravery;

        [Header("Bonfire")]
        [SerializeField] private int minWoodToSurvive = 3;
        [SerializeField] private float bonfireRadius = 10f;
        [SerializeField] private LayerMask bonfireLayerMask;

        public int MinWoodToSurvive => minWoodToSurvive;
        public int BonfireLayerMask => bonfireLayerMask;
        public float BonfireRadius => bonfireRadius;
        
        [Header("Item")] 
        [SerializeField] private LayerMask itemLayerMask;
        [SerializeField] private LayerMask itemContainerLayerMask;
        [SerializeField] private float itemGatherRadius = 15f;
        [SerializeField] private List<InventorySpawnConfig> initialInventory = new();

        public int ItemLayerMask => itemLayerMask;
        public int ItemContainerLayerMask => itemContainerLayerMask;
        public float ItemGatherRadius => itemGatherRadius;
        public List<InventorySpawnConfig> InitialInventory => initialInventory;
    }

    [Serializable]
    public class InventorySpawnConfig
    {
        [SerializeField] private ItemType itemType = ItemType.Wood;
        [SerializeField] private List<int> spawnAmounts = new();
        public ItemType ItemType => itemType;
        public List<int> SpawnAmounts => spawnAmounts;
    }
}