using System.Collections.Generic;
using Day;
using Dialogue;
using Inventory.Signals;
using Map.Actor;
using Signals;
using Sirenix.OdinInspector;
using Tutorial;
using UnityEngine;
using Zenject;

namespace LD48.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private List<TutorialEntry> tutorials = new();

        [Inject] private IDayNightCycle dayNightCycle;
        [Inject] private IMapActorRegistry mapActorRegistry;

        [ShowInInspector] [ReadOnly] private List<TutorialEntry> shownTutorials = new();

        private TutorialConditionPayload tutorialPayload = new();

        private const int KFirstDayNumber = 1;

        private void OnEnable()
        {
            SignalsHub.AddListener<DayNightCycleUpdatedSignal>(OnDayNightCycleUpdated);
            SignalsHub.AddListener<PlayerInventoryUpdatedEvent>(OnPlayerInventoryUpdated);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<DayNightCycleUpdatedSignal>(OnDayNightCycleUpdated);
            SignalsHub.RemoveListener<PlayerInventoryUpdatedEvent>(OnPlayerInventoryUpdated);
        }

        private void OnDayNightCycleUpdated(DayNightCycleUpdatedSignal signal)
        {
            ShowMatchingTutorial();
        }

        private void OnPlayerInventoryUpdated(PlayerInventoryUpdatedEvent signal)
        {
            ShowMatchingTutorial();
        }

        private void ShowMatchingTutorial()
        {
            foreach (var tutorial in tutorials)
            {
                if (shownTutorials.Contains(tutorial)) continue;
                if (!tutorial.IsReadyToShow(GetTutorialPayload())) continue;

                shownTutorials.Add(tutorial);
                SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(tutorial.DialogueEntry));
                break;
            }
        }

        private TutorialConditionPayload GetTutorialPayload()
        {
            var playerActor = mapActorRegistry.Player.ValueOrDefault();
            return new TutorialConditionPayload
            {
                DayNumber = dayNightCycle.CurrentDay,
                DayTime = dayNightCycle.CurrentCycle,
                Inventory = playerActor.Inventory,
                MaybeHandItem = playerActor.HandItem,
                Visibility = playerActor.Visibility,
            };
        }
    }
}