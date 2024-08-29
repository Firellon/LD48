using UnityEngine;

namespace Journal.JournalPanel
{
    public class JournalPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject journalPanel;
        [SerializeField] private RectTransform journalPanelContent;
        [SerializeField] private GameObject journalEntryPrefab;

        public GameObject JournalPanel => journalPanel;
        public RectTransform JournalPanelContent => journalPanelContent;
        public GameObject JournalEntryPrefab => journalEntryPrefab; // TODO: Store this in a UI Prefab Registry instead
    }
}