using System;
using System.Collections.Generic;
using Day;
using UnityEngine;
using Zenject;

namespace Lighting
{
    public class ShadowController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ambientShadow;
        [SerializeField] private List<SpriteRenderer> directionalShadows = new List<SpriteRenderer>();

        [Inject] private IDayNightCycle dayNightCycle;

        private float shadowIntensity = 1f;
        
        private const double K_intensityEpsilon = 0.01f;
        void Update()
        {
            var newShadowIntensity = 1 - dayNightCycle.TargetIntensity;
            if (Math.Abs(this.shadowIntensity - newShadowIntensity) > K_intensityEpsilon)
            {
                UpdateShadowIntensity(newShadowIntensity);
            }
        }

        private void UpdateShadowIntensity(float shadowIntensity)
        {
            this.shadowIntensity = shadowIntensity;
            var shadowColor = ambientShadow.color;
            shadowColor = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowIntensity);
            ambientShadow.color = shadowColor;
        }
    }
}
