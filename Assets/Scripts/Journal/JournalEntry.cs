using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Journal
{
    [CreateAssetMenu(menuName = "LD48/Create JournalEntry SO", fileName = "JournalEntry", order = 0)]
    public class JournalEntry : ScriptableObject
    {
        [SerializeField] private string entryKey;
        [SerializeField] private int entryOrder;
        [FormerlySerializedAs("entryName")] [SerializeField] private string entryTitle;
        [TextArea(minLines: 5, maxLines: 20), SerializeField, InspectorTextArea] private string entryDescription; 
        
        public string EntryKey => entryKey;
        public int EntryOrder => entryOrder;
       [ShowInInspector,ReadOnly] public string EntryShortName => $"#{entryOrder}";
        public string EntryTitle => entryTitle;
        public string EntryDescription => entryDescription;

        public bool Equals(JournalEntry entry)
        {
            return entry.EntryKey == EntryKey;
        }
    }
}