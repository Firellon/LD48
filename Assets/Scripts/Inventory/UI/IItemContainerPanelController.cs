namespace Inventory.UI
{
    public interface IItemContainerPanelController
    {
        void Show();
        void Hide();
        bool IsVisible { get; }
        void SetUp(IItemContainer itemContainer);
    }
}