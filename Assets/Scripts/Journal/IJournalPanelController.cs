namespace Journal
{
    public interface IJournalPanelController
    {
        bool IsVisible { get; }
        void Hide();
        void Show();
    }
}