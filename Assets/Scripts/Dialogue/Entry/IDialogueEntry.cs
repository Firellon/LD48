using System.Collections.Generic;

namespace Dialogue.Entry
{
    public interface IDialogueEntry
    {
        string EntryKey { get; }
        List<DialogueEntryReplica> Replicas { get; }
    }
}