using System;

namespace Pagination
{
    public class PaginationConfiguration : IPaginationConfiguration
    {
        public Action<int> OnPreviousPage { get; set; }
        public Action<int> OnNextPage { get; set;  }
        public int PageCount { get; set; }
        public int StartingPageIndex { get; set; }
        
        public bool ShowCurrentPageText { get; set; }
    }
}