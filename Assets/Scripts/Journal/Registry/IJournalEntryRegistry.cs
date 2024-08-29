using System.Collections.Generic;
using Utilities.Monads;

namespace Journal
{
    public interface IJournalEntryRegistry
    {
        IMaybe<JournalEntry> GetEntryOrEmpty(string entryKey);
        JournalEntry GetEntry(string entryKey);
        IReadOnlyList<JournalEntry> Entries { get; }
    }
}