namespace Player
{
    public class PlayerInputEnabledEvent
    {
        public bool IsEnabled { get; }
        
        public PlayerInputEnabledEvent(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
        
    }
}