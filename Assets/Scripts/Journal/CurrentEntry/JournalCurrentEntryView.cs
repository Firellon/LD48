using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Journal.JournalCurrentEntry
{
    public class JournalCurrentEntryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI entryNameText;
        [SerializeField] private TextMeshProUGUI entryDescriptionText;
        [SerializeField] private RectTransform entryContainer;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;

        public TextMeshProUGUI EntryNameText => entryNameText;
        public TextMeshProUGUI EntryDescriptionText => entryDescriptionText;
        public RectTransform EntryContainer => entryContainer;
        public Button PreviousPageButton => previousPageButton;
        public Button NextPageButton => nextPageButton;

        public void Refresh()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(RecalculateHeightCoroutine());
            }
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