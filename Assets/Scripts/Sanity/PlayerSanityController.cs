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
        [SerializeField] private int sanityLoss = 2;
        [SerializeField] private double darknessVisibilityThreshold = 0.5f;

        private float timeInDarkness;

        private void Update()
        {
            if (lightEventListener)
            {
                if (timeInDarkness >= sanityLossInterval)
                {
                    timeInDarkness -= sanityLossInterval;
                    humanController.State.Sanity -= sanityLoss;
                    SignalsHub.DispatchAsync(new PlayerSanityUpdatedEvent(humanController.State.Sanity));

                    CheckSanityDeath();
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

        private void CheckSanityDeath()
        {
            if (humanController.State.Sanity <= HumanState.K_MinSanity && !humanController.State.IsDead)
            {
                humanController.Die();
            }
        }
    }
}