using LD48;

namespace Human
{
    public class InteractableEnterEvent
    {
        public HumanController HumanController { get; }
        public IInteractable Interactable { get; }
        
        public InteractableEnterEvent(HumanController humanController, IInteractable interactable)
        {
            HumanController = humanController;
            Interactable = interactable;
        }
    }
}