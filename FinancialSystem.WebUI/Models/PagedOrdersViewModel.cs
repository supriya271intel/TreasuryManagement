using System.Collections.Generic;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.WebUI.Models
{
    public class PagedOrdersViewModel
    {
        public IEnumerable<Order> Orders { get; set; }

        public int CurrentPage  { get; set; }
        public int TotalPages   { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize     { get; set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext     => CurrentPage < TotalPages;

        public int From => TotalRecords == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        public int To   => CurrentPage == TotalPages ? TotalRecords : CurrentPage * PageSize;
    }
}
