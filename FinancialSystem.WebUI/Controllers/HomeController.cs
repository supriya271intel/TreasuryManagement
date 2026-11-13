using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinancialSystem.WebUI.Models;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Infrastructure.Data;

namespace FinancialSystem.WebUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // ── Fetch live data directly from Database ───────────────────────
            var orders = await _db.Orders
                .Include(o => o.Instrument)
                .AsNoTracking()
                .ToListAsync();

            var trades = await _db.Trades
                .Include(t => t.BuyOrder).ThenInclude(o => o.Instrument)
                .Include(t => t.SellOrder).ThenInclude(o => o.Instrument)
                .AsNoTracking()
                .ToListAsync();

            var instruments = await _db.Instruments
                .AsNoTracking()
                .ToListAsync();

            var fixedDeposits = await _db.FixedDeposits
                .AsNoTracking()
                .ToListAsync();

            // ── Instrument Breakdown (Mutual Funds, G-Secs, Fixed Deposits) ──
            int mfCount = instruments.OfType<MutualFund>().Count();
            int gsecCount = instruments.OfType<GSec>().Count();
            int fdInstCount = instruments.OfType<FixedDepositInstrument>().Count();
            int fdCount = Math.Max(fdInstCount, fixedDeposits.Count);

            int totalInstruments = mfCount + gsecCount + fdCount;

            // ── Reference Business Date (User Business Date or Today) ────────
            var today = DateTime.Today;
            var bDateClaim = User.Claims.FirstOrDefault(c => c.Type == "BusinessDate")?.Value;
            if (!string.IsNullOrEmpty(bDateClaim) && DateTime.TryParse(bDateClaim, out var parsedBDate))
            {
                today = parsedBDate.Date;
            }

            // ── Last 7 Days Trade Value (Aggregated from DB Trades) ──────────
            var dateLabels = new string[7];
            var dailyValues = new decimal[7];
            for (int i = 6; i >= 0; i--)
            {
                var day = today.AddDays(-i);
                dateLabels[6 - i] = day.ToString("MMM dd");
                dailyValues[6 - i] = trades
                    .Where(t => t.TradeDate.Date == day)
                    .Sum(t => t.TradePrice * t.TradeQuantity);
            }

            // Fallback if trades exist but fall on dates earlier than current window
            if (dailyValues.All(v => v == 0) && trades.Any())
            {
                var latestTradeDate = trades.Max(t => t.TradeDate).Date;
                for (int i = 6; i >= 0; i--)
                {
                    var day = latestTradeDate.AddDays(-i);
                    dateLabels[6 - i] = day.ToString("MMM dd");
                    dailyValues[6 - i] = trades
                        .Where(t => t.TradeDate.Date == day)
                        .Sum(t => t.TradePrice * t.TradeQuantity);
                }
            }

            // ── Order KPIs from Database ────────────────────────────────────
            int totalOrders = orders.Count;
            int pendingOrders = orders.Count(o => o.Status == OrderStatus.Pending);
            int matchedOrders = orders.Count(o => o.Status == OrderStatus.Matched);
            int rejectedOrders = orders.Count(o => o.Status == OrderStatus.Rejected);

            // ── Trade KPIs from Database ────────────────────────────────────
            int totalTrades = trades.Count;
            int settledTrades = trades.Count(t => t.Status == SettlementStatus.Settled);
            int pendingTrades = trades.Count(t => t.Status == SettlementStatus.Pending);
            int failedTrades = trades.Count(t => t.Status == SettlementStatus.Failed);

            decimal totalTradeValue = trades.Sum(t => t.TradePrice * t.TradeQuantity);

            // ── Recent Trades Table from Database ───────────────────────────
            var recentTrades = trades
                .OrderByDescending(t => t.TradeDate)
                .ThenByDescending(t => t.Id)
                .Take(5)
                .ToList();

            var vm = new DashboardViewModel
            {
                // Order KPIs
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                MatchedOrders = matchedOrders,
                RejectedOrders = rejectedOrders,

                // Trade KPIs
                TotalTrades = totalTrades,
                SettledTrades = settledTrades,
                PendingTrades = pendingTrades,
                FailedTrades = failedTrades,

                TotalTradeValue = totalTradeValue,
                TotalInstruments = totalInstruments,

                // Chart data: Order Status Doughnut
                OrderStatusCounts = new[]
                {
                    pendingOrders,
                    matchedOrders,
                    rejectedOrders
                },

                // Chart data: Settlement Status Doughnut
                TradeStatusCounts = new[]
                {
                    pendingTrades,
                    settledTrades,
                    failedTrades
                },

                TradeDateLabels = dateLabels,
                TradeDailyValues = dailyValues,

                // Chart data: Instrument Mix Bar Chart (Mutual Funds, G-Secs, Fixed Deposits)
                MutualFundCount = mfCount,
                GSecCount = gsecCount,
                FixedDepositCount = fdCount,

                RecentTrades = recentTrades
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
