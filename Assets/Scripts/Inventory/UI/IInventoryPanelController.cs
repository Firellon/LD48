namespace Inventory.UI
{
    public interface IInventoryPanelController
    {
        void Show();
        void Hide();
        bool IsVisible { get; }
    }
}