namespace Environment
{
    public class MapDiaryCollectedSignal
    {
        public MapJournalEntry MapJournalEntry { get; }

        public MapDiaryCollectedSignal(MapJournalEntry mapJournalEntry)
        {
            MapJournalEntry = mapJournalEntry;
        }
    }
}