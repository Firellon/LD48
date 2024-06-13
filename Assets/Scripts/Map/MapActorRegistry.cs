using LD48;
using Player;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Monads;

namespace Map
{
    public class MapActorRegistry : MonoBehaviour, IMapActorRegistry
    {
        [ShowInInspector, ReadOnly] private IMaybe<PlayerController> player = Maybe.Empty<PlayerController>();

        public IMaybe<PlayerController> Player => player;

        public void SetPlayer(PlayerController newPlayer)
        {
            player = Maybe.Of(newPlayer);
        }
    }
}