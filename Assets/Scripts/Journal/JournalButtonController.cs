using Environment;
using Inventory.UI;
using Signals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Journal
{
    public class JournalButtonController : MonoBehaviour 
    {
        [SerializeField] private Button journalButton;
        
        [Inject] private IJournalPanelController journalPanelController;
        
        private void OnEnable()
        {
            journalButton.onClick.AddListener(ToggleJournal);
        }
        
        private void OnDisable()
        {
            journalButton.onClick.RemoveListener(ToggleJournal);
            SignalsHub.RemoveListener<MapDiaryCollectedSignal>(OnDiaryCollected);
        }

        private void OnDiaryCollected(MapDiaryCollectedSignal signal)
        {
            // TODO: Add diary to the list of displayed diaries
        }

        private void ToggleJournal()
        {
            if (journalPanelController.IsVisible)
            {
                journalPanelController.Hide();
            }
            else
            {
                journalPanelController.Show();
            }
        }
    }
}