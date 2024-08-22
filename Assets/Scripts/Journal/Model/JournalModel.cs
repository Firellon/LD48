using System;
using System.Collections.Generic;
using System.Linq;
using Environment;
using LD48;
using ModestTree;
using Signals;
using UnityEngine;
using Utilities.RandomService;
using Zenject;
using IInitializable = Zenject.IInitializable;

namespace Journal
{
    public class JournalModel : IJournalModel, IInitializable, IDisposable
    {
        [Inject] private IJournalEntryRegistry journalEntryRegistry;
        [Inject] private IRandomService randomService;

        public List<JournalEntry> UnlockedEntries { get; } = new();

        public void Initialize()
        {
            SignalsHub.AddListener<MapDiaryCollectedSignal>(OnMapDiaryCollected);
        }

        public void Dispose()
        {
            SignalsHub.RemoveListener<MapDiaryCollectedSignal>(OnMapDiaryCollected);
        }
        
        private void OnMapDiaryCollected(MapDiaryCollectedSignal signal)
        {
            var lockedEntries = journalEntryRegistry.Entries.Except(UnlockedEntries).ToList();
            if (lockedEntries.None())
            {
                Debug.Log("OnMapDiaryCollected > no more Journal Entries to unlock!");
                return;
            }

            var unlockedEntry = randomService.Sample(lockedEntries);
            UnlockedEntries.Add(unlockedEntry);
            SignalsHub.DispatchAsync(new JournalEntryUnlockedSignal(unlockedEntry));
        }
    }
}