using Stranger;
using Unity.VisualScripting;
using UnityEngine;

namespace Dialogue.Entry
{
    [CreateAssetMenu(menuName = "LD48/Create DialogueEntry SO", fileName = "New Dialogue Entry", order = 0)]
    public class DialogueEntry : ScriptableObject, IDialogueEntry
    {
        [SerializeField] private string entryKey;
        [SerializeField] private string entryTitle;
        [TextArea(minLines: 5, maxLines: 20), SerializeField, InspectorTextArea] private string entryDescription;
        [SerializeField] private Character character;

        public string EntryKey => entryKey;
        public string EntryTitle => entryTitle;
        public string EntryDescription => entryDescription;
        public Character EntryCharacter => character;
    }
}