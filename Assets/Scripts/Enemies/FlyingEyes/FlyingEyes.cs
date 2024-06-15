using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FunkyCode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LD48.Enemies
{
    public class FlyingEyes : MonoBehaviour
    {
        [SerializeField] private LightEventListener lightEventListener;

        [SerializeField] private List<Light2D> eyes;

        [SerializeField,ColorUsageAttribute(true, true)] private Color normalColor;
        [SerializeField,ColorUsageAttribute(true, true)] private Color blinkColor;

        private void OnEnable()
        {
            StartCoroutine(nameof(BlinkProcess));
        }

        private IEnumerator BlinkProcess()
        {
            while (true)
            {
                while (lightEventListener.visability > 0.2f)
                {
                    SetEyesColorImmediately(blinkColor);
                    yield return null;
                }

                SetEyesColorImmediately(normalColor);

                yield return new WaitForSeconds(Random.Range(2f, 5f));

                var blinkCounts = 1;

                if (Random.value < 0.2f)
                {
                    blinkCounts++;
                }

                for (int i = 0; i < blinkCounts; i++)
                {
                    yield return SetEyesColor(normalColor, blinkColor, 0.1f);

                    yield return new WaitForSeconds(0.2f);

                    yield return SetEyesColor(blinkColor, normalColor, 0.1f);
                }
            }
        }

        private IEnumerator SetEyesColor(Color startColor, Color endColor, float time)
        {
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