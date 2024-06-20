using System.Collections;
using DG.Tweening;
using FunkyCode;
using Map;
using UnityEngine;
using Utilities.Prefabs;
using Utilities.RandomService;
using Zenject;

namespace LD48.Enemies
{
    public class FlyingEyesController : MonoBehaviour
    {
        [SerializeField] private FloatMinMax delay;

        [SerializeField] private GameObject flyingEyesPrefab;
        [SerializeField] private AnimationCurve animationCurve;

        [SerializeField] private float moveSpeed = 2f;
        
        [Inject] private IMapActorRegistry mapActorRegistry;
        [Inject] private IRandomService randomService;
        [Inject] private IPrefabPool prefabPool;

        [Inject] private ILightCycle lightCycle;

        [Inject] private IEnemiesHelper enemiesHelper;

        private void OnEnable()
        {
            StartCoroutine(nameof(ShowFlyingEyesProcess));
        }

        private IEnumerator ShowFlyingEyesProcess()
        {
            while (true)
            {
                if (lightCycle.Time < 0.9f)
                {
                    yield return null;
                }

                var currentDelay = randomService.Float(delay.Min, delay.Max);

                yield return new WaitForSeconds(currentDelay);

                var (startPoint, endPoint) = enemiesHelper.FindPathNearCharacter();

                var moveTime = Vector2.Distance(startPoint, endPoint) / moveSpeed;

                var eyes = prefabPool.Spawn(flyingEyesPrefab, transform);
                eyes.transform.position = startPoint;

                var moves = randomService.Int(3, 5);

                moveTime /= moves;

                for (var i = 1; i <= moves; i++)
                {
                    var start = Vector2.Lerp(startPoint, endPoint, (i-1) / (float)moves);
                    var end = Vector2.Lerp(startPoint, endPoint, i / (float)moves);

                    eyes.transform.position = start;

                    yield return DOTween.Sequence()
                        .Append(eyes.transform
                            .DOMoveX(end.x, moveTime))
                        .Insert(0f, eyes.transform
                            .DOMoveY(end.y, moveTime)
                            .SetEase(animationCurve))
                        .WaitForCompletion();

                    yield return new WaitForSeconds(randomService.Float(0.5f, 2f));
                }

                prefabPool.Despawn(eyes);
            }
        }
    }
}