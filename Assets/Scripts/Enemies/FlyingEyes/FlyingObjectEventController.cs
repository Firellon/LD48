using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FunkyCode;
using Map.Actor;
using UnityEngine;
using Utilities.Prefabs;
using Utilities.RandomService;
using Zenject;

namespace LD48.Enemies
{
    public enum FlyingObjectMovementType
    {
        Steps,
        Continuos,
        Manual,
        WalkingAround,
    }

    [Serializable]
    public class FlyingObjectItemOptions
    {
        public float StepsDelay;
    }

    [Serializable]
    public class FlyingObjectItem
    {
        public FlyingObjectMovementType MovementType;
        public AnimationCurve AnimationCurve;
        public GameObject Prefab;
        public float MoveSpeed;

        public FlyingObjectItemOptions Options;
    }

    public class FlyingObjectEventController : MonoBehaviour
    {
        [SerializeField] private FloatMinMax delay;

        [SerializeField] private List<FlyingObjectItem> items;

        [Inject] private IMapActorRegistry mapActorRegistry;
        [Inject] private IRandomService randomService;
        [Inject] private IPrefabPool prefabPool;

        [Inject] private ILightCycle lightCycle;
        [Inject] private IEnemiesHelper enemiesHelper;

        private void OnEnable()
        {
            StartCoroutine(nameof(ShowFlyingObjectProcess));
        }

        private IEnumerator ShowFlyingObjectProcess()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (lightCycle.Time < 0.9f)
                {
                    yield return null;
                }

                var currentDelay = randomService.Float(delay.Min, delay.Max);

                yield return new WaitForSeconds(currentDelay);

                var (startPoint, endPoint) = enemiesHelper.FindPathNearCharacter();

                var item = randomService.Sample(items);
                var moveTime = Vector2.Distance(startPoint, endPoint) / item.MoveSpeed;

                var flyingObject = prefabPool.Spawn(item.Prefab, transform);
                flyingObject.transform.position = startPoint;

                if (item.MovementType == FlyingObjectMovementType.Steps)
                {
                    var moves = randomService.Int(3, 5);

                    moveTime /= moves;

                    for (var i = 1; i <= moves; i++)
                    {
                        var start = Vector2.Lerp(startPoint, endPoint, (i - 1) / (float)moves);
                        var end = Vector2.Lerp(startPoint, endPoint, i / (float)moves);

                        flyingObject.transform.position = start;

                        yield return DOTween.Sequence()
                            .Append(flyingObject.transform
                                .DOMoveX(end.x, moveTime))
                            .Insert(0f, flyingObject.transform
                                .DOMoveY(end.y, moveTime)
                                .SetEase(item.AnimationCurve))
                            .WaitForCompletion();

                        yield return new WaitForSeconds(randomService.Float(0.5f, 2f));
                    }
                }

                if (item.MovementType == FlyingObjectMovementType.Continuos)
                {
                    yield return DOTween.Sequence()
                        .Append(flyingObject.transform
                            .DOMoveX(endPoint.x, moveTime))
                        .Insert(0f, flyingObject.transform
                            .DOMoveY(endPoint.y, moveTime)
                            .SetEase(item.AnimationCurve))
                        .WaitForCompletion();
                }

                if (item.MovementType == FlyingObjectMovementType.Manual)
                {
                    flyingObject.transform.position = startPoint;
                    yield break;
                }

                if (item.MovementType == FlyingObjectMovementType.WalkingAround)
                {
                    var moves = randomService.Int(3, 5);

                    for (var i = 1; i <= moves; i++)
                    {
                        var nextPoint = enemiesHelper.FindPointAround(startPoint, 2f);

                        flyingObject.transform.position = startPoint;

                        moveTime = Vector2.Distance(startPoint, nextPoint) / item.MoveSpeed;

                        yield return DOTween.Sequence()
                            .Append(flyingObject.transform
                                .DOMoveX(nextPoint.x, moveTime))
                            .Insert(0f, flyingObject.transform
                                .DOMoveY(nextPoint.y, moveTime)
                                .SetEase(item.AnimationCurve))
                            .WaitForCompletion();

                        yield return new WaitForSeconds(item.Options.StepsDelay);

                        startPoint = nextPoint;
                    }
                }

                prefabPool.Despawn(flyingObject);
            }
        }
    }
}