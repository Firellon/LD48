using Player;
using Utilities.Monads;

namespace Map
{
    public interface IMapActorRegistry
    {
        IMaybe<PlayerController> Player { get; }
        void SetPlayer(PlayerController newPlayer);
    }
}