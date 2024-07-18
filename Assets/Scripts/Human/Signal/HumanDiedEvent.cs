namespace Human.Signal
{
    public class HumanDiedEvent
    {
        public HumanController Human { get; }

        public HumanDiedEvent(HumanController humanController)
        {
            Human = humanController;
        }
    }
}