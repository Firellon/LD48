using System.Collections.Generic;

namespace Journal
{
    public interface IJournalModel
    {
        List<JournalEntry> UnlockedEntries { get; }
    }
}