using System;
using Human;
using LD48;
using Signals;
using Utilities.Monads;
using Zenject;

namespace Hint
{
    public class HintController : IInitializable, IDisposable
    {
        private readonly HintView hintView;

        [Inject]
        public HintController(HintView hintView)
        {
            this.hintView = hintView;
        }
        
        public void Initialize()
        {
            hintView.PressEHintView.SetActive(false);
            
            SignalsHub.AddListener<InteractableEnterEvent>(OnInteractableEnter);
            SignalsHub.AddListener<InteractableExitEvent>(OnInteractableExit);
        }

        public void Dispose()
        {
            SignalsHub.RemoveListener<InteractableEnterEvent>(OnInteractableEnter);
            SignalsHub.RemoveListener<InteractableExitEvent>(OnInteractableExit);
        }

        private void OnInteractableEnter(InteractableEnterEvent evt)
        {
            OnInteractableUpdate(evt.HumanController);
        }

        private void OnInteractableExit(InteractableExitEvent evt)
        {
            OnInteractableUpdate(evt.HumanController);
        }

        private void OnInteractableUpdate(HumanController humanController)
        {
            if (!humanController.IsPlayer) return;
            
            var maybeInteractable = humanController.InteractableObjects.FirstOrEmpty(CanInteract);
            hintView.PressEHintView.SetActive(maybeInteractable.IsPresent);
            maybeInteractable.IfPresent(interactable => {
                hintView.PressEHintViewTarget = interactable.GameObject.transform;
            }).IfNotPresent(() => {
                hintView.PressEHintViewTarget = null;
            });
        }
        
        private bool CanInteract(IInteractable obj)
        {
            return obj.CanInteract();
        }
    }
}