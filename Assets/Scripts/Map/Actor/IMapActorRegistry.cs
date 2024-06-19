using Player;
using Utilities.Monads;

namespace Map.Actor
{
    public interface IMapActorRegistry
    {
        IMaybe<PlayerController> Player { get; }
        void SetPlayer(PlayerController newPlayer);
        
        IMaybe<MapActor> GetMapActorOrEmpty(MapActorType actorType);
        MapActor GetMapActor(MapActorType actorType);
    }
}