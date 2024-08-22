using TMPro;
using UnityEngine;
using Utilities.Monads;

namespace Journal
{
    public class JournalCurrentEntryController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI entryNameText;
        [SerializeField] private TextMeshProUGUI entryDescriptionText;

        public void SetUp(IMaybe<JournalEntry> maybeJournalEntry)
        {
            maybeJournalEntry.IfPresent(journalEntry =>
            {
                entryNameText.text = journalEntry.EntryName;
                entryDescriptionText.text = journalEntry.EntryDescription;
            }).IfNotPresent(() =>
            {
                entryNameText.text = string.Empty;
                entryDescriptionText.text = string.Empty;
            });
        }
    }
}