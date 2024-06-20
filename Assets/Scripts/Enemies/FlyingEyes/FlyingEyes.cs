using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FunkyCode;
using Map.Actor;
using UnityEngine;
using Zenject;

namespace LD48.Enemies
{
    public class FlyingEyes : MonoBehaviour
    {
        [SerializeField] private float timeBetweenBlinks = 1f;
        [SerializeField] private float lightingThresholdValue = 0.1f;
        [SerializeField] private float maxDistanceToCharacter = 2f;

        [SerializeField] private LightEventListener lightEventListener;

        [SerializeField] private List<Light2D> eyes;

        [SerializeField,ColorUsageAttribute(true, true)] private Color normalColor;
        [SerializeField,ColorUsageAttribute(true, true)] private Color blinkColor;

        [Inject] private IMapActorRegistry mapActorRegistry;

        private bool isBlinking = false;

        private float globalAlphaValue;

        private float nextBlinkTime;

        private void OnEnable()
        {
            isBlinking = false;
            nextBlinkTime = Time.time + timeBetweenBlinks;
        }

        private void Update()
        {
            if (isBlinking)
                return;

            // if (Time.time > nextBlinkTime)
            // {
            //     StartCoroutine(nameof(BlinkProcess));
            // }

            SetEyesColorImmediately(GetCurrentColor());
        }

        private Color GetCurrentColor()
        {
            var distanceToCharacter = Vector2.Distance(
                transform.position,
                mapActorRegistry.Player.ValueOrDefault().transform.position
            );

            var lightingLevel = lightEventListener.visability > lightingThresholdValue ? 1f : 0f;

            if (distanceToCharacter < maxDistanceToCharacter)
            {
                lightingLevel = 1f;
            }

            return Color.Lerp(normalColor, blinkColor, lightingLevel);
        }

        private IEnumerator BlinkProcess()
        {
            var blinkCounts = 1;

            isBlinking = true;

            for (int i = 0; i < blinkCounts; i++)
            {
                yield return SetEyesColor(GetCurrentColor(), blinkColor, 0.1f);

                yield return new WaitForSeconds(0.1f);

                yield return SetEyesColor(blinkColor, GetCurrentColor(), 0.1f);
            }

            nextBlinkTime = Time.time + timeBetweenBlinks;

            isBlinking = false;
        }

        private IEnumerator SetEyesColor(Color startColor, Color endColor, float time)
        {
            var startColorWithAlpha = startColor;
            startColorWithAlpha.a *= globalAlphaValue;

            var endColorWithAlpha = endColor;
            endColorWithAlpha.a *= globalAlphaValue;

            foreach (var light2D in eyes)
            {
                DOVirtual.Color(startColor, endColor, time, value => { light2D.color = value; });
            }

            yield return new WaitForSeconds(time);
        }

        private void SetEyesColorImmediately(Color color)
        {
            foreach (var light2D in eyes)
            {
                light2D.color = color;
            }
        }
    }
}