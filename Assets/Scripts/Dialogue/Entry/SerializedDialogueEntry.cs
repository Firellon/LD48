using System;
using Stranger;

namespace Dialogue.Entry
{
    [Serializable]
    public class SerializedDialogueEntry : IDialogueEntry
    {
        public string EntryKey { get; set; }
        public string EntryTitle { get; set; }
        public string EntryDescription { get; set; }
        public Character EntryCharacter { get; set; }
    }
}