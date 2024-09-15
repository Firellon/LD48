using System;
using JetBrains.Annotations;
using Stranger;

namespace Dialogue.Entry
{
    public interface IDialogueEntry
    {
        string EntryKey { get; }
        string EntryTitle { get; }
        string EntryDescription { get; }
        [CanBeNull] Character EntryCharacter { get; }
    }
}