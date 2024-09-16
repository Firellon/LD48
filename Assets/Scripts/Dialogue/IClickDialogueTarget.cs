using Dialogue.Entry;
using Inventory.UI;
using UI;
using UnityEngine.EventSystems;

namespace Dialogue
{
    public interface IClickDialogueTarget : IPointerClickHandler, IHighlightable
    {
        IDialogueEntry DialogueEntry { get; }
    }
}