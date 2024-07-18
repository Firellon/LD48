namespace UI.Signals
{
    public class SetPlayerTipCommand
    {
        public string PlayerTipText { get; }

        public SetPlayerTipCommand(string playerTipText)
        {
            PlayerTipText = playerTipText;
        }
    }
}