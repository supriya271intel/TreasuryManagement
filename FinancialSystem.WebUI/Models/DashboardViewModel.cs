using System;
using System.Collections.Generic;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.WebUI.Models
{
    public class DashboardViewModel
    {
        // KPI Cards
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int MatchedOrders { get; set; }
        public int RejectedOrders { get; set; }

        public int TotalTrades { get; set; }
        public int SettledTrades { get; set; }
        public int PendingTrades { get; set; }
        public int FailedTrades { get; set; }

        public decimal TotalTradeValue { get; set; }
        public int TotalInstruments { get; set; }

        // Chart data: Orders by status (for doughnut)
        public int[] OrderStatusCounts { get; set; }   // [Pending, Matched, Rejected]

        // Chart data: Trade settlement status (for doughnut)
        public int[] TradeStatusCounts { get; set; }   // [Pending, Settled, Failed]

        // Chart data: Trade value per day (last 7 days) for line chart
        public string[] TradeDateLabels { get; set; }
        public decimal[] TradeDailyValues { get; set; }

        // Chart data: Instrument type split (for bar chart)
        public int MutualFundCount { get; set; }
        public int GSecCount { get; set; }
        public int FixedDepositCount { get; set; }

        // Recent trades table
        public List<Trade> RecentTrades { get; set; } = new List<Trade>();
    }
}
