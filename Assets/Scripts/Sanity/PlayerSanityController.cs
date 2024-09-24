using FunkyCode;
using Human;
using Journal.JournalPanel;
using LD48;
using Sanity.Signals;
using Signals;
using UnityEngine;
using Utilities.Monads;
using Zenject;

namespace Sanity
{
    public class PlayerSanityController : MonoBehaviour
    {
        [Inject] private HumanController humanController;

        [SerializeField] private LightEventListener lightEventListener;
        [SerializeField] private float sanityLossInterval = 5f;
        [SerializeField] private int sanityLoss = 2;
        [SerializeField] private int sanityGain = 1;
        [SerializeField] private double darknessVisibilityThreshold = 0.5f;

        private float timeInDarkness;
        private float timeSpentReading;
        private bool journalPanelShown;

        private void OnEnable()
        {
            SignalsHub.AddListener<JournalPanelShownEvent>(OnJournalPanelShown);
            SignalsHub.AddListener<JournalPanelHiddenEvent>(OnJournalPanelHidden);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<JournalPanelShownEvent>(OnJournalPanelShown);
            SignalsHub.RemoveListener<JournalPanelHiddenEvent>(OnJournalPanelHidden);
        }

        private void OnJournalPanelShown(JournalPanelShownEvent evt)
        {
            journalPanelShown = true;
        }
        
        private void OnJournalPanelHidden(JournalPanelHiddenEvent evt)
        {
            journalPanelShown = false;
        }

        private void Start()
        {
            SignalsHub.DispatchAsync(new PlayerSanityUpdatedEvent(humanController.State.Sanity));
        }

        private void Update()
        {
            if (lightEventListener != null)
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
                    if (lightEventListener.visability > darknessVisibilityThreshold)
                    {
                        if (timeInDarkness > 0)
                        {
                            Debug.Log($"Visability {lightEventListener.visability} > {darknessVisibilityThreshold} => playerLit");
                            SignalsHub.DispatchAsync(new PlayerLitEvent());
                        }
                        timeInDarkness = 0;
                        CheckSanityRestoration();
                    }
                    else
                    {
                        
                        if (timeInDarkness <= 0)
                        {
                            Debug.Log($"Visability {lightEventListener.visability} <= {darknessVisibilityThreshold} => playerUnlit");
                            SignalsHub.DispatchAsync(new PlayerUnlitEvent());
                        }
                        timeInDarkness += Time.deltaTime;
                    }
                }
            }
        }

        private void CheckSanityRestoration()
        {
            if (!humanController.IsResting) return;
            if (timeSpentReading < sanityLossInterval)
            {
                timeSpentReading += Time.deltaTime;
                return;
            }

            var isReading = humanController.Inventory.HandItem.Match(handItem => handItem.ItemType == ItemType.Book, false) 
                            || journalPanelShown;

            if (isReading)
            {
                humanController.State.Sanity += sanityGain;
                SignalsHub.DispatchAsync(new PlayerSanityUpdatedEvent(humanController.State.Sanity));
                timeSpentReading -= sanityLossInterval;
            }
        }

        private void CheckSanityDeath()
        {
            if (humanController.State.Sanity <= HumanState.K_MinSanity && !humanController.State.IsDead)
            {
                humanController.Die(CauseOfDeath.Madness);
            }
        }
    }
}