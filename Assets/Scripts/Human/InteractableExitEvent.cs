using LD48;

namespace Human
{
    public class InteractableExitEvent
    {
        public HumanController HumanController { get; }
        public IInteractable Interactable { get; }
        
        public InteractableExitEvent(HumanController humanController, IInteractable interactable)
        {
            HumanController = humanController;
            Interactable = interactable;
        }
    }
}