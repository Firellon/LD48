using System;
using System.Collections;
using System.Collections.Generic;
using Day;
using DG.Tweening;
using FunkyCode;
using Map.Actor;
using Signals;
using Sirenix.OdinInspector;
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
        [SerializeField, MinMaxSlider(0f, 1f)] private float randomEventChance = 0.05f;

        [SerializeField] private List<FlyingObjectItem> items;

        [Inject] private IMapActorRegistry mapActorRegistry;
        [Inject] private IRandomService randomService;
        [Inject] private IPrefabPool prefabPool;

        [Inject] private ILightCycle lightCycle;
        [Inject] private IDayNightCycle dayNightCycle;
        [Inject] private IEnemiesHelper enemiesHelper;

        private void OnEnable()
        {
            SignalsHub.AddListener<DayNightCycleChangedSignal>(OnDayNightCycleChangedSignal);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<DayNightCycleChangedSignal>(OnDayNightCycleChangedSignal);
        }

        private void OnDayNightCycleChangedSignal(DayNightCycleChangedSignal signal)
        {
            if (signal.Cycle != DayTime.Night)
                return;

            StartCoroutine(nameof(TryShowFlyingObject));
        }

        private IEnumerator TryShowFlyingObject()
        {
            if (!randomService.Chance(randomEventChance))
                yield break;

            Debug.LogWarning("Generating a random event!");

            var nightLength = dayNightCycle.CurrentCycleLength;

            var randomEventMoment = randomService.Float(0f, nightLength);

            yield return new WaitForSeconds(randomEventMoment);

            yield return GenerateFlyingObject();
        }

        private IEnumerator GenerateFlyingObject()
        {
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