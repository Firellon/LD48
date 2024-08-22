using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Journal
{
    public class JournalEntryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI entryNameText;
        [SerializeField] private Button entryButton;
        
        public void SetUp(JournalEntry journalEntry)
        {
            entryNameText.text = journalEntry.EntryName;
            entryButton.onClick.RemoveAllListeners();
            entryButton.onClick.AddListener(() => OpenJournalEntry(journalEntry));
        }

        private void OpenJournalEntry(JournalEntry entry)
        {
            SignalsHub.DispatchAsync(new OpenJournalEntryCommand(entry));
        }
    }
}