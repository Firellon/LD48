using Stranger;

namespace Dialogue.Entry
{
    public interface IDialogueEntry
    {
        string EntryKey { get; }
        string EntryTitle { get; }
        string EntryDescription { get; }
        Character EntryCharacter { get; }
    }
}