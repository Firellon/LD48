using System;
using Player;
using Signals;
using Stranger;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Dialogue
{
    public class DialogueController : IInitializable, IDisposable
    {
        private readonly DialogueView view;
        private readonly ICharacterRegistry characterRegistry;
        
        private bool isShown = false;
        private float dialogueShownAt;
        private const double HideDialogueOnPlayerMoveSeconds = 1;

        [Inject]
        private DialogueController(DialogueView view, ICharacterRegistry characterRegistry, PlayerControls playerInput)
        {
            this.view = view;
            this.characterRegistry = characterRegistry;
            
            view.gameObject.SetActive(false);
            
            playerInput.UI.Enable();
            playerInput.UI.Click.performed += OnMouseClick;
            
            SignalsHub.AddListener<PlayerMovedEvent>(OnPlayerMoved);
        }

        private void OnMouseClick(InputAction.CallbackContext obj)
        {
            if (isShown) HideDialogueEntry();
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
            isShown = true;
            dialogueShownAt = Time.time;

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
        }

        private void OnHideDialogueEntry(HideDialogueEntryCommand command)
        {
            HideDialogueEntry();
        }
        
        private void OnPlayerMoved(PlayerMovedEvent evt)
        {
            if (Time.time >= dialogueShownAt + HideDialogueOnPlayerMoveSeconds)
            {
                HideDialogueEntry();
            }
        }

        private void HideDialogueEntry()
        {
            if (!isShown) return;
            view.gameObject.SetActive(false); // TODO: Fade-out animation
            isShown = false;
        }
    }
}