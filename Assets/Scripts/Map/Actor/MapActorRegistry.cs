using System.Collections.Generic;
using System.Linq;
using Player;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Monads;

namespace Map.Actor
{
    public class MapActorRegistry : MonoBehaviour, IMapActorRegistry
    {
        [ShowInInspector, ReadOnly] private IMaybe<PlayerController> player = Maybe.Empty<PlayerController>();
        [SerializeField] private List<MapActor> mapActors = new();

        public IMaybe<PlayerController> Player => player;

        public void SetPlayer(PlayerController newPlayer)
        {
            player = Maybe.Of(newPlayer);
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