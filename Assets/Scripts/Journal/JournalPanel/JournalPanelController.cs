using System.Collections.Generic;
using System.Linq;
using Signals;
using UnityEngine;
using Utilities;
using Utilities.Monads;
using Utilities.Prefabs;
using Zenject;

namespace Journal.JournalPanel
{
    public class JournalPanelController : MonoBehaviour, IJournalPanelController
    {
        [SerializeField] private GameObject journalPanel;
        [SerializeField] private RectTransform journalPanelContent;
        [SerializeField] private GameObject journalEntryPrefab;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IJournalModel journalModel;
        [Inject] private JournalCurrentEntryController journalCurrentEntryController;
        public bool IsVisible => journalPanel.activeSelf;

        private List<JournalEntry> unlockedJournalEntries = new();
        private IMaybe<JournalEntry> currentUnlockedEntry = Maybe.Empty<JournalEntry>();

        private void OnEnable()
        {
            SignalsHub.AddListener<JournalEntryUnlockedSignal>(OnJournalEntryUnlocked);
            SignalsHub.AddListener<OpenJournalEntryCommand>(OpenJournalEntry);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<JournalEntryUnlockedSignal>(OnJournalEntryUnlocked);
            SignalsHub.RemoveListener<OpenJournalEntryCommand>(OpenJournalEntry);
        }
        
        private void OnJournalEntryUnlocked(JournalEntryUnlockedSignal signal)
        {
            UpdateJournalPanelItems();
        }
        
        private void OpenJournalEntry(OpenJournalEntryCommand signal)
        {
            if (!IsVisible)
            {
                Show();
            }
            
            currentUnlockedEntry = unlockedJournalEntries.Contains(signal.Entry) 
                ? Maybe.Of(signal.Entry) 
                : Maybe.Empty<JournalEntry>();
            journalCurrentEntryController.SetUp(currentUnlockedEntry);
        }

        private void Start()
        {
            journalPanelContent.DestroyChildren();
            Hide();
        }

        public void Show()
        {
            if (IsVisible) return;

            journalPanel.SetActive(true); // TODO: Show animation

            UpdateJournalPanelItems();
            if (unlockedJournalEntries.Any())
            {
                SignalsHub.DispatchAsync(new OpenJournalEntryCommand(unlockedJournalEntries.First()));
            }
            
            SignalsHub.DispatchAsync(new JournalPanelShownEvent());
        }

        public void Hide()
        {
            if (!IsVisible) return;

            journalPanel.SetActive(false); // TODO: Hide animation
            SignalsHub.DispatchAsync(new JournalPanelHiddenEvent());
        }
        
        private void UpdateJournalPanelItems()
        {
            journalPanelContent.DespawnChildren(prefabPool);
            unlockedJournalEntries = journalModel.UnlockedEntries.OrderBy(entry => entry.EntryOrder).ToList();
            foreach (var journalEntry in unlockedJournalEntries)
            {
                var journalEntryView = prefabPool.Spawn(journalEntryPrefab, journalPanelContent);
                journalEntryView.GetComponent<JournalEntryView>().SetUp(journalEntry);
            }
        }
    }
}