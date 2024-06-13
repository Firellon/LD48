namespace Sanity
{
    public class PlayerSanityUpdatedEvent
    {
        public int Sanity { get; }
        
        public PlayerSanityUpdatedEvent(int sanity)
        {
            Sanity = sanity;
        }
    }
}