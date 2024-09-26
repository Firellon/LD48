using Day;
using Dialogue;
using Dialogue.Entry;
using Signals;
using UnityEngine;

namespace LD48.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private DialogueEntry firstDayTutorial;
        [SerializeField] private DialogueEntry firstNightComingTutorial;
        [SerializeField] private DialogueEntry craftingTutorial;

        private void OnEnable()
        {
            SignalsHub.AddListener<DayNightCycleUpdatedSignal>(OnDayNightCycleUpdated);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<DayNightCycleUpdatedSignal>(OnDayNightCycleUpdated);
        }

        private void OnDayNightCycleUpdated(DayNightCycleUpdatedSignal signal)
        {
            if (ShouldShowFirstDayTutorial(signal))
            {
                SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(firstDayTutorial));
            }
            
            if (ShouldShowFirstNightComingTutorial(signal))
            {
                SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(firstNightComingTutorial));
            }

            if (ShouldShowCraftingTutorial(signal))
            {
                SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(craftingTutorial));
            }
        }

        private bool ShouldShowFirstDayTutorial(DayNightCycleUpdatedSignal signal)
        {
            return signal.CurrentDay == 1 && signal.DayCycle == DayTime.Day;
        }
        
        private bool ShouldShowFirstNightComingTutorial(DayNightCycleUpdatedSignal signal)
        {
            return signal.CurrentDay == 1 && signal.DayCycle == DayTime.NightComing;
        }
        
        private bool ShouldShowCraftingTutorial(DayNightCycleUpdatedSignal signal)
        {
            // TODO: Define
            return false;
        }
    }
}