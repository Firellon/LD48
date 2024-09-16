using Dialogue.Entry;

namespace Dialogue
{
    public class ShowDialogueEntryCommand
    {
        public IDialogueEntry DialogueEntry { get; }
        
        public ShowDialogueEntryCommand(IDialogueEntry dialogueEntry)
        {
            DialogueEntry = dialogueEntry;
        }
    }
    
    public class HideDialogueEntryCommand
    {
        public HideDialogueEntryCommand()
        {
        }
    }
}