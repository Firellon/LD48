using System.Collections;
using TMPro;
using UnityEngine;
using Utilities.Monads;

namespace Journal
{
    public class JournalCurrentEntryController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI entryNameText;
        [SerializeField] private TextMeshProUGUI entryDescriptionText;
        [SerializeField] private RectTransform entryContainer;

        public void SetUp(IMaybe<JournalEntry> maybeJournalEntry)
        {
            maybeJournalEntry.IfPresent(journalEntry =>
            {
                entryNameText.text = journalEntry.EntryTitle;
                entryDescriptionText.text = journalEntry.EntryDescription;
            }).IfNotPresent(() =>
            {
                entryNameText.text = string.Empty;
                entryDescriptionText.text = string.Empty;
            });

            if (gameObject.activeInHierarchy) StartCoroutine(RecalculateHeightCoroutine());
        }

        private IEnumerator RecalculateHeightCoroutine()
        {
            yield return new WaitForEndOfFrame();
            RecalculateHeight();
        }

        private void RecalculateHeight()
        {
            var entryNameTextHeight = ((RectTransform) entryNameText.transform).sizeDelta.y;
            var entryDescriptionTextHeight = ((RectTransform) entryDescriptionText.transform).sizeDelta.y;
            const int entrySpacing = 20;
            entryContainer.sizeDelta =
                new Vector2(entryContainer.sizeDelta.x,
                    entryNameTextHeight + entrySpacing + entryDescriptionTextHeight);
        }
    }
}