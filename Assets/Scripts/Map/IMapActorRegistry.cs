using LD48;
using Utilities.Monads;

namespace Map
{
    public interface IMapActorRegistry
    {
        IMaybe<Player> Player { get; }
        void SetPlayer(Player newPlayer);
    }
}