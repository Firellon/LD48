using System;
using System.Linq;
using Environment;
using Journal.ToggleButton;
using Player;
using Signals;
using Zenject;

namespace Journal
{
    public class JournalToggleButtonController : IInitializable, IDisposable
    {
        private readonly JournalToggleButtonView view;
        private readonly IJournalModel model;
        private readonly IJournalPanelController panelController;

        [Inject]
        public JournalToggleButtonController(
            JournalToggleButtonView view,
            IJournalModel model,
            IJournalPanelController panelController)
        {
            this.view = view;
            this.model = model;
            this.panelController = panelController;
        }
        
        public void Initialize()
        {
            view.JournalButton.onClick.AddListener(ToggleJournal);
            
            SignalsHub.AddListener<MapDiaryCollectedSignal>(OnDiaryCollected);
            SignalsHub.AddListener<PlayerMovedEvent>(OnPlayerMovedEvent);
            SignalsHub.AddListener<PlayerActedEvent>(OnPlayerActedEvent);

            UpdateViewVisibility();
        }

        public void Dispose()
        {
            view.JournalButton.onClick.RemoveListener(ToggleJournal);
            
            SignalsHub.RemoveListener<MapDiaryCollectedSignal>(OnDiaryCollected);
            SignalsHub.RemoveListener<PlayerMovedEvent>(OnPlayerMovedEvent);
            SignalsHub.RemoveListener<PlayerActedEvent>(OnPlayerActedEvent);
        }

        private void OnDiaryCollected(MapDiaryCollectedSignal signal)
        {
            // Show a notification over the journal entry button?
            UpdateViewVisibility();
        }
        
        private void OnPlayerMovedEvent(PlayerMovedEvent signal)
        {
            if (panelController.IsVisible)
            {
                panelController.Hide();
            }
        }
        
        private void OnPlayerActedEvent(PlayerActedEvent signal)
        {
            if (panelController.IsVisible)
            {
                panelController.Hide();
            }
        }

        private void UpdateViewVisibility()
        {
            view.gameObject.SetActive(model.UnlockedEntries.Any());
        }

        private void ToggleJournal()
        {
            if (panelController.IsVisible)
            {
                panelController.Hide();
            }
            else
            {
                panelController.Show();
            }
        }
    }
}