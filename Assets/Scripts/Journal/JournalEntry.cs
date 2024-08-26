using UnityEngine;

namespace Journal
{
    [CreateAssetMenu(menuName = "LD48/Create JournalEntry SO", fileName = "JournalEntry", order = 0)]
    public class JournalEntry : ScriptableObject
    {
        [SerializeField] private string entryKey;
        [SerializeField] private int entryOrder;
        [SerializeField] private string entryName;
        [TextArea(minLines: 5, maxLines: 20), SerializeField] private string entryDescription; 
        
        public string EntryKey => entryKey;
        public int EntryOrder => entryOrder;
        public string EntryName => entryName;
        public string EntryDescription => entryDescription;

        public bool Equals(JournalEntry entry)
        {
            return entry.EntryKey == EntryKey;
        }
    }
}