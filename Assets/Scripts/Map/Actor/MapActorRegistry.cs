using System;
using System.Collections.Generic;
using System.Linq;
using Human.Signal;
using Player;
using Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Monads;

namespace Map.Actor
{
    public class MapActorRegistry : MonoBehaviour, IMapActorRegistry
    {
        [ShowInInspector, ReadOnly] private IMaybe<PlayerController> maybePlayer = Maybe.Empty<PlayerController>();
        [SerializeField] private List<MapActor> mapActors = new();

        public IMaybe<PlayerController> Player => maybePlayer;

        private void OnEnable()
        {
            SignalsHub.AddListener<HumanDiedEvent>(OnHumanDied);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<HumanDiedEvent>(OnHumanDied);
        }
        
        private void OnHumanDied(HumanDiedEvent evt)
        {
            maybePlayer.IfPresent(player =>
            {
                if (evt.Human.gameObject == player.gameObject)
                {
                    maybePlayer = Maybe.Empty<PlayerController>();
                    Destroy(evt.Human.gameObject);
                }
            });
        }

        public void SetPlayer(PlayerController newPlayer)
        {
            maybePlayer = Maybe.Of(newPlayer);
        }
        
        public IMaybe<MapActor> GetMapActorOrEmpty(MapActorType actorType)
        {
            return mapActors.FirstOrEmpty(actor => actor.MapActorType == actorType);
        }

        public MapActor GetMapActor(MapActorType actorType)
        {
            return mapActors.First(mapActor => mapActor.MapActorType == actorType);
        }
    }
}