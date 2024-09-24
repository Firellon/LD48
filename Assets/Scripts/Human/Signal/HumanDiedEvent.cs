namespace Human.Signal
{
    public class HumanDiedEvent
    {
        public HumanController Human { get; }
        public CauseOfDeath CauseOfDeath { get; }

        public HumanDiedEvent(HumanController humanController, CauseOfDeath causeOfDeath)
        {
            Human = humanController;
            CauseOfDeath = causeOfDeath;
        }
    }
}