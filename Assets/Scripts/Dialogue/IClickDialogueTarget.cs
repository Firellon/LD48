using Dialogue.Entry;
using UnityEngine.EventSystems;

namespace Dialogue
{
    public interface IClickDialogueTarget : IPointerClickHandler
    {
        IDialogueEntry DialogueEntry { get; }
    }
}