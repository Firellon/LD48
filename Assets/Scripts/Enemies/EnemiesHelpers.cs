using DITools;
using Map;
using UnityEngine;
using Utilities.RandomService;
using Zenject;

namespace LD48.Enemies
{
    public class EnemiesHelpers : IEnemiesHelper, IContainerConstructable
    {
        private Vector2 viewSize = new Vector2(14, 8);

        [Inject] private IRandomService randomService;
        [Inject] private IMapActorRegistry mapActorRegistry;

        public (Vector3 startPoint, Vector3 endPoint) FindPathNearCharacter()
        {
            var playerPosition = mapActorRegistry.Player.ValueOrDefault().transform.position;

            Vector3 startPoint, endPoint;

            var offsetX = viewSize.x / 4f;
            var radius = viewSize.y / 2f * 1.5f;

            var isTop = randomService.Chance(0.5f);
            var isLeft = randomService.Chance(0.5f);

            var angleDiff = 30f;

            var angle = isTop ? randomService.Float(90f - angleDiff, 90f + angleDiff) : randomService.Float(270 - angleDiff, 270 + angleDiff);
            var startRoundPoint = Quaternion.Euler(0f, 0f, angle) * Vector3.right * radius;
            var endRoundPoint = Quaternion.Euler(0f, 0f, angle + 180f) * Vector3.right * radius;

            var centerPosition = isLeft ? (playerPosition - Vector3.right * offsetX) : (playerPosition + Vector3.right * offsetX);

            startPoint = startRoundPoint + centerPosition;
            endPoint = endRoundPoint + centerPosition;

            return (startPoint, endPoint);
        }
    }

    public interface IEnemiesHelper
    {
        (Vector3 startPoint, Vector3 endPoint) FindPathNearCharacter();
    }
}