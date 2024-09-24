using System;
using Dialogue.Entry;

namespace Dialogue
{
    public class ShowDialogueEntryCommand
    {
        public IDialogueEntry DialogueEntry { get; }
        public Action OnClosed { get; }
        
        public ShowDialogueEntryCommand(IDialogueEntry dialogueEntry, Action onClosed = null)
        {
            DialogueEntry = dialogueEntry;
            OnClosed = onClosed;
        }
    }
    
    public class HideDialogueEntryCommand
    {
        public HideDialogueEntryCommand()
        {
        }
    }
}