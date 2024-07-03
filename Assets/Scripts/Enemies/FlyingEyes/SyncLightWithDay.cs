﻿using FunkyCode;
using UnityEngine;
using Utilities.RandomService;
using Zenject;

namespace LD48.Enemies
{
    public class SyncLightWithDay : MonoBehaviour
    {
        [SerializeField] private bool flickering;

        [SerializeField] private float flickersPerSecond = 15f;

        [SerializeField] private float flickerRangeMin = -0.1f;
        [SerializeField] private float flickerRangeMax = 0.1f;

        [SerializeField] private Light2D light;

        [SerializeField] private AudioSource audioSource;

        [Inject] private ILightCycle lightCycle;
        [Inject] private IRandomService randomService;

        private float lightAlpha;
        private float audioDefaultVolume;

        private float nextFlickTime;

        private void Awake()
        {
            lightAlpha = light.color.a;
            audioDefaultVolume = audioSource.volume;
        }

        private void LateUpdate()
        {
            var tempAlpha = lightAlpha;

            if (flickering && Time.time > nextFlickTime)
            {
                var diff = randomService.Float(flickerRangeMin, flickerRangeMax);

                tempAlpha += diff;
                tempAlpha = Mathf.Clamp01(tempAlpha);

                nextFlickTime = Time.time + 1f / flickersPerSecond;
            }

            light.color.a = lightCycle.Time > 0.5f ? tempAlpha * lightCycle.Time : 0f;
            audioSource.volume = lightCycle.Time > 0.5f ? audioDefaultVolume * lightCycle.Time : 0f;
        }
    }
}