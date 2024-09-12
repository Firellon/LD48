using System;
using Journal;
using Signals;
using Unity.VisualScripting;
using Zenject;
using IInitializable = Zenject.IInitializable;

namespace Dialogue
{
    public class DialogueController : IInitializable, IDisposable
    {
        private readonly DialogueView view;

        [Inject]
        private DialogueController(DialogueView view)
        {
            this.view = view;
            
            view.gameObject.SetActive(false);
        }

        public void Initialize()
        {
            SignalsHub.AddListener<ShowDialogueEntryCommand>(OnShowDialogueEntry);
            SignalsHub.AddListener<HideDialogueEntryCommand>(OnHideDialogueEntry);
        }

        public void Dispose()
        {
            SignalsHub.RemoveListener<ShowDialogueEntryCommand>(OnShowDialogueEntry);
            SignalsHub.RemoveListener<HideDialogueEntryCommand>(OnHideDialogueEntry);
        }
        
        private void OnShowDialogueEntry(ShowDialogueEntryCommand command)
        {
            var entry = command.DialogueEntry;

            view.gameObject.SetActive(true); // TODO: Fade-in animation
            
            view.CharacterNameText.text = entry.EntryTitle;
            view.CharacterLineText.text = entry.EntryDescription;
            view.CharacterPortraitImage.sprite = entry.EntryCharacter.Portrait;
        }

        private void OnHideDialogueEntry(HideDialogueEntryCommand command)
        {
            view.gameObject.SetActive(false); // TODO: Fade-out animation
        }
    }
}