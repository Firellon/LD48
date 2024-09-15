using System;
using System.Threading.Tasks;
using Signals;
using Stranger;
using Zenject;

namespace Dialogue
{
    public class DialogueController : IInitializable, IDisposable
    {
        private readonly DialogueView view;
        private readonly ICharacterRegistry characterRegistry;
        
        private bool shouldHide = false;

        private const double KShowDialogueDurationSeconds = 3;
        
        [Inject]
        private DialogueController(DialogueView view, ICharacterRegistry characterRegistry)
        {
            this.view = view;
            this.characterRegistry = characterRegistry;
            
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
        
        private async void OnShowDialogueEntry(ShowDialogueEntryCommand command)
        {
            var entry = command.DialogueEntry;

            view.gameObject.SetActive(true); // TODO: Fade-in animation
            shouldHide = true;

            if (entry.EntryCharacter == null && characterRegistry.PlayerCharacter.IsNotPresent)
            {
                throw new Exception(
                    "No Player Character found to replace the null provided in the Show Dialogue command!");
            }
            
            view.CharacterPortraitImage.sprite = entry.EntryCharacter != null 
                ? entry.EntryCharacter.Portrait 
                : characterRegistry.PlayerCharacter.ValueOrDefault().Portrait;
            view.CharacterNameText.text = entry.EntryTitle != string.Empty 
                ? entry.EntryTitle
                : characterRegistry.PlayerCharacter.ValueOrDefault().CharacterName;;
            view.CharacterLineText.text = entry.EntryDescription;

            await Task.Delay(TimeSpan.FromSeconds(KShowDialogueDurationSeconds));
            HideDialogueEntry();
        }

        private void OnHideDialogueEntry(HideDialogueEntryCommand command)
        {
            HideDialogueEntry();
        }

        private void HideDialogueEntry()
        {
            if (!shouldHide) return;
            view.gameObject.SetActive(false); // TODO: Fade-out animation
            shouldHide = false;
        }
    }
}