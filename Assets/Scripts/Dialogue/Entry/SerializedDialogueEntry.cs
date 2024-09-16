using System;
using Stranger;

namespace Dialogue.Entry
{
    [Serializable]
    public class SerializedDialogueEntry : IDialogueEntry
    {
        public string EntryKey { get; set; } = string.Empty;
        public string EntryTitle { get; set; } = string.Empty;
        public string EntryDescription { get; set; } = string.Empty;
        public Character EntryCharacter { get; set; }
    }
}