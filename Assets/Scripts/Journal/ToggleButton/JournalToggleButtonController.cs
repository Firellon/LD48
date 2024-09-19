using System;
using System.Linq;
using Dialogue;
using Dialogue.Entry;
using Environment;
using Journal.ToggleButton;
using Player;
using Sanity.Signals;
using Signals;
using UnityEngine;
using Zenject;

namespace Journal
{
    public class JournalToggleButtonController : IInitializable, IDisposable
    {
        private readonly JournalToggleButtonView view;
        private readonly IJournalModel model;
        private readonly IJournalPanelController panelController;
        private readonly IDialogueEntry tooDarkCantReadDialogueEntry;

        private bool playerIsLit = true;

        [Inject]
        public JournalToggleButtonController(
            JournalToggleButtonView view,
            IJournalModel model,
            IJournalPanelController panelController,
            IDialogueEntry tooDarkCantReadDialogueEntry)
        {
            this.view = view;
            this.model = model;
            this.panelController = panelController;
            this.tooDarkCantReadDialogueEntry = tooDarkCantReadDialogueEntry;
        }
        
        public void Initialize()
        {
            view.JournalButton.onClick.AddListener(ToggleJournal);
            
            SignalsHub.AddListener<MapDiaryCollectedSignal>(OnDiaryCollected);
            SignalsHub.AddListener<JournalEntryUnlockedSignal>(OnJournalEntryUnlocked);
            SignalsHub.AddListener<PlayerMovedEvent>(OnPlayerMovedEvent);
            SignalsHub.AddListener<PlayerActedEvent>(OnPlayerActedEvent);
            SignalsHub.AddListener<PlayerLitEvent>(OnPlayerLit);
            SignalsHub.AddListener<PlayerUnlitEvent>(OnPlayerUnlit);

            UpdateViewVisibility();
        }

        public void Dispose()
        {
            view.JournalButton.onClick.RemoveListener(ToggleJournal);
            
            SignalsHub.RemoveListener<MapDiaryCollectedSignal>(OnDiaryCollected);
            SignalsHub.RemoveListener<JournalEntryUnlockedSignal>(OnJournalEntryUnlocked);
            SignalsHub.RemoveListener<PlayerMovedEvent>(OnPlayerMovedEvent);
            SignalsHub.RemoveListener<PlayerActedEvent>(OnPlayerActedEvent);
            SignalsHub.RemoveListener<PlayerLitEvent>(OnPlayerLit);
            SignalsHub.RemoveListener<PlayerUnlitEvent>(OnPlayerUnlit);
        }

        private void OnDiaryCollected(MapDiaryCollectedSignal signal)
        {
            // Show a notification over the journal entry button?
            UpdateViewVisibility();
        }
        
        private void OnJournalEntryUnlocked(JournalEntryUnlockedSignal signal)
        {
            if (playerIsLit)
            {
                Debug.Log("OnJournalEntryUnlocked > playerIsLit, opening the Journal");
                SignalsHub.DispatchAsync(new OpenJournalEntryCommand(signal.UnlockedEntry));   
            }
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
        
        private void OnPlayerLit(PlayerLitEvent signal)
        {
            playerIsLit = true;
        }
        
        private void OnPlayerUnlit(PlayerUnlitEvent signal)
        {
            playerIsLit = false;
            if (panelController.IsVisible)
            {
                panelController.Hide();
                SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(tooDarkCantReadDialogueEntry));
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
                if (playerIsLit)
                {
                    panelController.Show();   
                }
                else
                {
                    SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(tooDarkCantReadDialogueEntry));
                }
            }
        }
    }
}