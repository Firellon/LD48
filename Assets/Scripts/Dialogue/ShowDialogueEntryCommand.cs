namespace Dialogue
{
    public class ShowDialogueEntryCommand
    {
        public DialogueEntry DialogueEntry { get; }
        
        public ShowDialogueEntryCommand(DialogueEntry dialogueEntry)
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