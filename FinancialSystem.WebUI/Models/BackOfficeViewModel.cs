using System;
using System.Collections.Generic;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.WebUI.Models
{
    public class BackOfficeViewModel
    {
        public IEnumerable<Trade> Trades { get; set; } = new List<Trade>();
        
        // Date filter window: [Current Date - 7 days, Current Date + 7 days]
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CurrentDate { get; set; } = DateTime.Today;

        public string ActiveFilter { get; set; } = "window"; // "window", "pending", "settled", "all"

        // Queue metrics
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int SettledCount { get; set; }
        public int CbsProcessedCount { get; set; }
    }

    public class TradeEditViewModel
    {
        public int TradeId { get; set; }
        public decimal TradePrice { get; set; }
        public int TradeQuantity { get; set; }
        public DateTime TradeDate { get; set; }
        public SettlementStatus Status { get; set; }
        public string ActionType { get; set; } = "save"; // "save" or "settle"
    }
}
