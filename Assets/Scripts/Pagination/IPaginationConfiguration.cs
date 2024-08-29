using System;

namespace Pagination
{
    public interface IPaginationConfiguration
    {
        public Action<int> OnPreviousPage { get; }
        public Action<int> OnNextPage { get; }
        public int PageCount { get; }
        public int StartingPageIndex { get; }
        
        public bool ShowCurrentPageText { get; }
    }
}