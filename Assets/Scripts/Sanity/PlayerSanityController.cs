using System;
using Day;
using FunkyCode;
using Human;
using Signals;
using UnityEngine;
using Zenject;

namespace Sanity
{
    public class PlayerSanityController : MonoBehaviour
    {
        [Inject] private HumanController humanController;
        [Inject] private IDayNightCycle dayNightCycle;

        [SerializeField] private LightEventListener lightEventListener;
        [SerializeField] private float sanityLossInterval = 5f;
        [SerializeField] private double darknessVisibilityThreshold = 0.5f;

        private float timeInDarkness;


        private void Update()
        {
            if (lightEventListener)
            {
                if (timeInDarkness >= sanityLossInterval)
                {
                    timeInDarkness -= sanityLossInterval;
                    humanController.State.Sanity -= 1;
                    SignalsHub.DispatchAsync(new PlayerSanityUpdatedEvent(humanController.State.Sanity));
                }
                else
                {
                    if (dayNightCycle.GetCurrentCycle() != DayTime.Night || lightEventListener.visability > darknessVisibilityThreshold)
                    {
                        timeInDarkness = 0;
                    }
                    else
                    {
                        // Debug.Log($"Visability {lightEventListener.visability} < {darknessVisibilityThreshold}");
                        timeInDarkness += Time.deltaTime;
                    }
                }
            }
        }
    }
}