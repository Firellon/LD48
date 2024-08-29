using System;
using Zenject;

namespace Pagination
{
    public class PaginationController : IPaginationController
    {
        private readonly PaginationView view;

        private IPaginationConfiguration configuration;
        private int currentIndex;

        private int FirstPageIndex => 0;
        private int LastPageIndex => configuration.PageCount - 1;

        [Inject]
        public PaginationController(PaginationView view)
        {
            this.view = view;
        }

        public void SetUp(IPaginationConfiguration paginationConfiguration)
        {
            configuration = paginationConfiguration;
            currentIndex = Math.Clamp(paginationConfiguration.StartingPageIndex, FirstPageIndex,
                Math.Max(FirstPageIndex, LastPageIndex));

            view.PreviousPageButton.onClick.RemoveAllListeners();
            view.PreviousPageButton.onClick.AddListener(OnPreviousPage);

            view.NextPageButton.onClick.RemoveAllListeners();
            view.NextPageButton.onClick.AddListener(OnNextPage);

            UpdateView();
        }

        private string GetPageNumberText()
        {
            return $"{currentIndex + 1} / {configuration.PageCount}";
        }

        private void OnPreviousPage()
        {
            if (currentIndex <= FirstPageIndex) return;
            currentIndex--;

            configuration.OnPreviousPage(currentIndex);
            UpdateView();
        }

        private void OnNextPage()
        {
            if (currentIndex >= LastPageIndex) return;
            currentIndex++;

            configuration.OnNextPage(currentIndex);
            UpdateView();
        }

        private void UpdateView()
        {
            var showPageButtons = configuration.PageCount > 1;
            view.PreviousPageButton.gameObject.SetActive(showPageButtons);
            view.NextPageButton.gameObject.SetActive(showPageButtons);

            view.PreviousPageButton.interactable = currentIndex > FirstPageIndex;
            view.NextPageButton.interactable = currentIndex < LastPageIndex;

            view.PageNumberText.gameObject.SetActive(configuration.ShowCurrentPageText);
            view.PageNumberText.text = GetPageNumberText();
        }
    }
}