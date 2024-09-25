using System;
using System.Collections.Generic;
using System.Linq;
using Environment.Signals;
using Inventory;
using LD48;
using Map;
using Map.Actor;
using ModestTree;
using Signals;
using UnityEngine;
using Utilities.Monads;
using Utilities.RandomService;
using Zenject;

namespace Player.Compass
{
    public class PlayerCompassController : IInitializable, IDisposable
    {
        private readonly PlayerCompassView view;
        private readonly IMapGenerator mapGenerator;
        private readonly IRandomService randomService;
        private readonly IMapActorRegistry mapActorRegistry;
        
        private List<Vector2Int> remainingKeySegments = new();
        private List<Vector2Int> remainingExitSegments = new();
        private IMaybe<Vector2Int> maybeCurrentWaypoint = Maybe.Empty<Vector2Int>();
        
        [Inject]
        public PlayerCompassController(PlayerCompassView view, IMapGenerator mapGenerator, IRandomService randomService, IMapActorRegistry mapActorRegistry)
        {
            this.view = view;
            this.mapGenerator = mapGenerator;
            this.randomService = randomService;
            this.mapActorRegistry = mapActorRegistry;
        }
        
        public void Initialize()
        {
            SignalsHub.AddListener<MapWaypointActivatedSignal>(OnMapWaypointActivated);
            SignalsHub.AddListener<MapItemRemovedEvent>(OnMapItemRemoved); 
            SignalsHub.AddListener<PlayerSetEvent>(OnPlayerSet);

            remainingKeySegments = mapGenerator.PotentialKeySegments;
            remainingExitSegments = mapGenerator.PotentialExitSegments;
            
            UpdateCurrentWaypoint();
        }

        public void Dispose()
        {
            SignalsHub.RemoveListener<MapWaypointActivatedSignal>(OnMapWaypointActivated);
            SignalsHub.RemoveListener<MapItemRemovedEvent>(OnMapItemRemoved); 
            SignalsHub.RemoveListener<PlayerSetEvent>(OnPlayerSet);
        }
        

        private void OnMapWaypointActivated(MapWaypointActivatedSignal signal)
        {
            var activatedSegment = mapGenerator.ConvertWorldPositionToSegmentPosition(signal.WaypointPosition);
            remainingExitSegments = remainingExitSegments.Except(activatedSegment).ToList();
            remainingKeySegments = remainingKeySegments.Except(activatedSegment).ToList();
            
            UpdateCurrentWaypoint();
        }
        
        private void OnMapItemRemoved(MapItemRemovedEvent evt)
        {
            if (evt.ItemType == ItemType.Key)
            {
                Debug.Log("OnMapItemRemoved > no more remaining key segments");
                remainingKeySegments = new List<Vector2Int>();
            }
        }
        
        private void OnPlayerSet(PlayerSetEvent evt)
        {
            UpdateCurrentWaypoint();
        }

        private void UpdateCurrentWaypoint()
        {
            // TODO: Check if the Player has the Key item here?
            if (remainingKeySegments.Any())
            {
                maybeCurrentWaypoint = randomService.Sample(remainingKeySegments).ToMaybe();
            }
            else if (remainingExitSegments.Any())
            {
                maybeCurrentWaypoint = randomService.Sample(remainingExitSegments).ToMaybe();
            }
            else
            {
                maybeCurrentWaypoint = Maybe.Empty<Vector2Int>();
            }

            view.CompassTarget = maybeCurrentWaypoint.Match(
                waypoint => mapGenerator.ConvertSegmentPositionToWorldPosition(waypoint).ToMaybe(), 
                Maybe.Empty<Vector3>);
            view.MaybePlayer = mapActorRegistry.Player;
        }
    }
}