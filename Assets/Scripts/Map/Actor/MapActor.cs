using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map.Actor
{
    [CreateAssetMenu(menuName = "LD48/Create MapActor SO", fileName = "New MapActor", order = 0)]
    public class MapActor : ScriptableObject
    {
        [SerializeField] private MapActorType mapActorType;
        [SerializeField] private List<GameObject> prefabs;
        
        public MapActorType MapActorType => mapActorType;
        public List<GameObject> Prefabs => prefabs;
        public GameObject Prefab => prefabs.First();

        public GameObject GetRandomPrefab()
        {
            return prefabs[Random.Range(0, prefabs.Count)]; // TODO: Use random service?
        }
    }
}