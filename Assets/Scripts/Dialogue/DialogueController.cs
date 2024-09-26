using System;
using Dialogue.Entry;
using Player;
using Signals;
using Stranger;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Monads;
using Zenject;

namespace Dialogue
{
    public class DialogueController : IInitializable, IDisposable
    {
        private readonly DialogueView view;
        private readonly ICharacterRegistry characterRegistry;
        
        private bool isShown = false;
        private float dialogueShownAt;
        private IMaybe<IDialogueEntry> maybeDialogueEntry = Maybe.Empty<IDialogueEntry>();
        private int currentReplicaIndex = -1;
        private Action onClosedAction;

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
            if (isShown)
            {
                ShowNextDialogueReplica();
            }
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
            if (entry == null)
            {
                throw new Exception(
                    "No Dialogue Entry provided!");
            }

            view.gameObject.SetActive(true); // TODO: Fade-in animation
            isShown = true;
            dialogueShownAt = Time.time;
            maybeDialogueEntry = entry.ToMaybe();
            onClosedAction = command.OnClosed;
            currentReplicaIndex = -1;
            
            ShowNextDialogueReplica();
        }

        private void ShowNextDialogueReplica()
        {
            maybeDialogueEntry.IfPresent(dialogueEntry =>
            {
                currentReplicaIndex++;
                if (dialogueEntry.Replicas.Count <= currentReplicaIndex)
                {
                    HideDialogueEntry();
                    return;
                }
                var currentReplica = dialogueEntry.Replicas[currentReplicaIndex];
                if (currentReplica.EntryCharacter == null && characterRegistry.PlayerCharacter.IsNotPresent)
                {
                    throw new Exception(
                        "No Player Character found to replace the null provided in the Show Dialogue command!");
                }

                var entryCharacter = currentReplica.EntryCharacter != null
                    ? currentReplica.EntryCharacter
                    : characterRegistry.PlayerCharacter.ValueOrDefault();
                view.CharacterPortraitImage.sprite = entryCharacter.Portrait;
                view.CharacterNameText.text = currentReplica.EntryTitle != string.Empty 
                    ? currentReplica.EntryTitle
                    : entryCharacter.CharacterName;
                view.CharacterLineText.text = currentReplica.EntryDescription;
                // TODO: Play a sound when showing a new replica
            });
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
            onClosedAction?.Invoke();
            onClosedAction = null;
        }
    }
}