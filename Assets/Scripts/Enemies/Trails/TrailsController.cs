using System.Collections.Generic;
using FunkyCode;
using Map.Actor;
using Unity.Mathematics;
using UnityEngine;
using Utilities.Prefabs;
using Utilities.RandomService;
using Zenject;

namespace Enemies.Trails
{
    public class TrailsController : MonoBehaviour
    {
        [SerializeField] private List<GameObject> trailsPrefabs;

        [SerializeField] private float timeBetweenSpawn = 60f * 5f;

        [SerializeField] private float generationRadius = 16f;

        [Inject] private ILightCycle lightCycle;
        [Inject] private IPrefabPool prefabPool;
        [Inject] private IRandomService randomService;
        [Inject] private IMapActorRegistry actorRegistry;

        private float lastSpawnTime;

        private void Update()
        {
            var lightTime = lightCycle.Time;

            // night
            if (lightTime > 0.95f)
            {
                if (Time.time < lastSpawnTime + timeBetweenSpawn)
                    return;

                lastSpawnTime = Time.time;

                var playerPosition = actorRegistry.Player.ValueOrDefault().transform.position;
                var trailPosition = playerPosition + randomService.PointOnCircleEdge(generationRadius);

                var trail = prefabPool.Spawn(randomService.Sample(trailsPrefabs));
                trail.transform.SetPositionAndRotation(trailPosition, quaternion.Euler(0f, 0f, randomService.Float(0f, 360f)));
                trail.transform.localScale = Vector3.one * (randomService.Chance(0.5f) ? -1 : 1);
            }
        }
    }
}