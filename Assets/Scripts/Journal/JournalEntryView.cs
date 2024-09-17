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

        [SerializeField] private Color selectedTextColor;
        [SerializeField] private Color unselectedTextColor;
        
        private JournalEntry entry;

        private void OnEnable()
        {
            SignalsHub.AddListener<OpenJournalEntryCommand>(OnOpenJournalEntry);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<OpenJournalEntryCommand>(OnOpenJournalEntry);
        }

        public void SetUp(JournalEntry journalEntry)
        {
            entry = journalEntry;
            entryNameText.text = journalEntry.EntryShortName;
            entryButton.onClick.RemoveAllListeners();
            entryButton.onClick.AddListener(() => OpenJournalEntry(journalEntry));
        }

        private void OnOpenJournalEntry(OpenJournalEntryCommand cmd)
        {
            entryNameText.color = cmd.Entry == entry ? selectedTextColor : unselectedTextColor;
        }

        private void OpenJournalEntry(JournalEntry entry)
        {
            SignalsHub.DispatchAsync(new OpenJournalEntryCommand(entry));
        }
    }
}