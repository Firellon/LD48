using LD48;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Monads;

namespace Map
{
    public class MapActorRegistry : MonoBehaviour, IMapActorRegistry
    {
        [ShowInInspector, ReadOnly] private IMaybe<Player> player = Maybe.Empty<Player>();

        public IMaybe<Player> Player => player;

        public void SetPlayer(Player newPlayer)
        {
            player = Maybe.Of(newPlayer);
        }
    }
}