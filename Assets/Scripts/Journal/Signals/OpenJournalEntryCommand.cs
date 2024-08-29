namespace Journal
{
    public class OpenJournalEntryCommand
    {
        public JournalEntry Entry { get; }

        public OpenJournalEntryCommand(JournalEntry entry)
        {
            Entry = entry;
        }
    }
}