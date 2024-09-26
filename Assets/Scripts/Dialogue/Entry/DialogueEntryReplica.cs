using System;
using Stranger;
using Unity.VisualScripting;
using UnityEngine;

namespace Dialogue.Entry
{
    [Serializable]
    public class DialogueEntryReplica
    {
        [SerializeField] private string entryTitle = string.Empty;
        [TextArea(minLines: 5, maxLines: 20), SerializeField, InspectorTextArea] private string entryDescription;
        [SerializeField] private Character character;
        
        public string EntryTitle
        {
            get => entryTitle;
            set => entryTitle = value;
        }

        public string EntryDescription
        {
            get => entryDescription;
            set => entryDescription = value;
        }

        public Character EntryCharacter
        {
            get => character;
            set => character = value;
        }
    }
}