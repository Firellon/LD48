using Dialogue.Entry;

namespace Dialogue
{
    public interface IClickDialogueTarget
    {
        IDialogueEntry DialogueEntry { get; }
    }
}