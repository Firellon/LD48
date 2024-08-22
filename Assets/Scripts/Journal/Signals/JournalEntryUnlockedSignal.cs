namespace Journal
{
    public class JournalEntryUnlockedSignal
    {
        public JournalEntry UnlockedEntry { get; }
        public JournalEntryUnlockedSignal(JournalEntry unlockedEntry)
        {
            UnlockedEntry = unlockedEntry;
        }
    }
}