﻿using DITools;
using Map.Actor;
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

            var offsetDivider = 3f;

            var offsetX = viewSize.x / offsetDivider;
            var radius = (viewSize.y / 2f) * 1.5f;

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

        public Vector3 FindPointAround(Vector3 startPoint, float radius)
        {
            var offsetDivider = 3f;

            var offsetX = viewSize.x / offsetDivider;

            var isTop = randomService.Chance(0.5f);
            var isLeft = randomService.Chance(0.5f);

            var angleDiff = 30f;

            var angle = isTop ? randomService.Float(90f - angleDiff, 90f + angleDiff) : randomService.Float(270 - angleDiff, 270 + angleDiff);
            var startRoundPoint = Quaternion.Euler(0f, 0f, angle) * Vector3.right * radius;

            var centerPosition = isLeft ? (startPoint - Vector3.right * offsetX) : (startPoint + Vector3.right * offsetX);

            startPoint = startRoundPoint + centerPosition;

            return startPoint;
        }
    }

    public interface IEnemiesHelper
    {
        (Vector3 startPoint, Vector3 endPoint) FindPathNearCharacter();
        Vector3 FindPointAround(Vector3 startPoint, float radius);
    }
}