﻿using System.Collections.Generic;
using System.Linq;
using Signals;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;
using Utilities.Monads;
using Utilities.Prefabs;
using Zenject;

namespace Journal
{
    public class JournalPanelController : MonoBehaviour, IJournalPanelController
    {
        [SerializeField] private GameObject journalPanel;
        [SerializeField] private RectTransform journalPanelContent;
        [SerializeField] private GameObject journalEntryPrefab;
        [SerializeField] private JournalCurrentEntryController journalCurrentEntryController;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IJournalModel journalModel;
        public bool IsVisible  => journalPanel.activeSelf;

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

            journalPanel.SetActive(true);

            UpdateJournalPanelItems();
        }

        public void Hide()
        {
            if (!IsVisible) return;

            journalPanel.SetActive(false);
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