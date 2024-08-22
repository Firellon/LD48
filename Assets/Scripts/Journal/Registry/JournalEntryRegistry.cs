using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.Monads;

namespace Journal
{
    public class JournalEntryRegistry : MonoBehaviour, IJournalEntryRegistry
    {
        [SerializeField] private List<JournalEntry> entries = new();

        public IMaybe<JournalEntry> GetEntryOrEmpty(string entryKey)
        {
            return entries.FirstOrEmpty(entry => entry.EntryKey == entryKey);
        }

        public JournalEntry GetEntry(string entryKey)
        {
            return entries.First(entry => entry.EntryKey == entryKey);
        }

        public IReadOnlyList<JournalEntry> Entries => entries;
    }
}