using System;
using System.Collections.Generic;

namespace Dialogue.Entry
{
    [Serializable]
    public class SerializedDialogueEntry : IDialogueEntry
    {
        public SerializedDialogueEntry()
        {
        }
        
        public SerializedDialogueEntry(string entryDescription)
        {
            Replicas = new List<DialogueEntryReplica>
            {
                new DialogueEntryReplica
                {
                    EntryDescription = entryDescription
                }
            };
        }

        public string EntryKey { get; set; } = string.Empty;
        public List<DialogueEntryReplica> Replicas { get; set; } = new();
    }
}