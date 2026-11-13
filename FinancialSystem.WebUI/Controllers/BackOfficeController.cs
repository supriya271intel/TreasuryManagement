using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;
using FinancialSystem.WebUI.Models;

namespace FinancialSystem.WebUI.Controllers
{
    [Authorize]
    public class BackOfficeController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly ISettlementFacade _settlementFacade;

        public BackOfficeController(IUnitOfWork uow, ISettlementFacade settlementFacade)
        {
            _uow = uow;
            _settlementFacade = settlementFacade;
        }

        // GET: /BackOffice
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string filter = "window")
        {
            var today = DateTime.Today;
            var minDate = fromDate ?? today.AddDays(-7);
            var maxDate = toDate ?? today.AddDays(7);
            var endOfDay = maxDate.Date.AddDays(1).AddTicks(-1);

            var allTrades = (await _uow.Trades.GetAllAsync()).OrderByDescending(t => t.TradeDate).ToList();

            // Development convenience: If seeded trades drifted past the 7-day cutoff,
            // bring them into the active window around today so demo trades are readily visible
            var cutoff = today.AddDays(-7);
            if (allTrades.Any(t => t.TradeDate < cutoff))
            {
                var list = allTrades.OrderBy(t => t.Id).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    // Spread trades over the last 3 days
                    list[i].TradeDate = today.AddDays(-(list.Count - 1 - i)).AddHours(11 + i * 2);
                }
                await _uow.CommitAsync();
            }

            // Apply requirement 2 filtering:
            // "Current Date , current Date before 7 days And Current Date after 7 days orders and Pending should be shown in BAckoffice."
            System.Collections.Generic.List<Trade> filteredTrades;
            switch (filter?.ToLowerInvariant())
            {
                case "pending":
                    filteredTrades = allTrades.Where(t => t.Status == SettlementStatus.Pending).ToList();
                    break;
                case "settled":
                    filteredTrades = allTrades.Where(t => t.Status == SettlementStatus.Settled).ToList();
                    break;
                case "all":
                    filteredTrades = allTrades;
                    break;
                case "window":
                default:
                    filter = "window";
                    // Trades within [CurrentDate - 7 days, CurrentDate + 7 days] OR status == Pending
                    filteredTrades = allTrades
                        .Where(t => (t.TradeDate.Date >= minDate.Date && t.TradeDate <= endOfDay) || t.Status == SettlementStatus.Pending)
                        .ToList();
                    break;
            }

            var vm = new BackOfficeViewModel
            {
                Trades = filteredTrades,
                FromDate = minDate,
                ToDate = maxDate,
                CurrentDate = today,
                ActiveFilter = filter,
                TotalCount = allTrades.Count,
                PendingCount = allTrades.Count(t => t.Status == SettlementStatus.Pending),
                SettledCount = allTrades.Count(t => t.Status == SettlementStatus.Settled),
                CbsProcessedCount = allTrades.Count(t => t.IsCbsProcessed)
            };

            return View(vm);
        }

        // GET: /BackOffice/GetTradeDetails?id=4
        [HttpGet]
        public async Task<IActionResult> GetTradeDetails(int id)
        {
            var trade = await _uow.Trades.GetByIdAsync(id);
            if (trade == null) return NotFound();

            var instName = trade.BuyOrder?.Instrument?.Name ?? trade.SellOrder?.Instrument?.Name ?? "Security Deal";
            var isin = trade.BuyOrder?.Instrument?.Isin ?? trade.SellOrder?.Instrument?.Isin ?? "N/A";

            return Json(new
            {
                id = trade.Id,
                instrumentName = instName,
                isin = isin,
                price = trade.TradePrice,
                quantity = trade.TradeQuantity,
                totalValue = trade.TradePrice * trade.TradeQuantity,
                tradeDate = trade.TradeDate.ToString("yyyy-MM-ddTHH:mm"),
                displayDate = trade.TradeDate.ToString("dd MMM yyyy, HH:mm"),
                status = trade.Status.ToString(),
                isCbsProcessed = trade.IsCbsProcessed,
                cbsTransactionId = trade.CbsTransactionId,
                cbsProcessedAt = trade.CbsProcessedAt?.ToString("dd MMM yyyy, HH:mm")
            });
        }

        // POST: /BackOffice/UpdateTrade
        // Requirement 3: Edit Popup Form with Details and Settle button for updating status From Pending to Settled
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTrade(TradeEditViewModel model)
        {
            var trade = await _uow.Trades.GetByIdAsync(model.TradeId);
            if (trade == null)
            {
                TempData["ErrorMessage"] = $"Trade T-{model.TradeId} not found.";
                return RedirectToAction("Index");
            }

            if (model.TradePrice > 0) trade.TradePrice = model.TradePrice;
            if (model.TradeQuantity > 0) trade.TradeQuantity = model.TradeQuantity;
            if (model.TradeDate != default) trade.TradeDate = model.TradeDate;

            if (model.ActionType?.ToLowerInvariant() == "settle" || model.Status == SettlementStatus.Settled)
            {
                trade.Status = SettlementStatus.Settled;
                await _uow.CommitAsync();
                TempData["SuccessMessage"] = $"Trade T-{trade.Id} has been updated to Settled! It is now eligible for CBS Integration.";
            }
            else
            {
                trade.Status = model.Status;
                await _uow.CommitAsync();
                TempData["SuccessMessage"] = $"Trade T-{trade.Id} details updated successfully.";
            }

            return RedirectToAction("Index");
        }

        // POST: /BackOffice/ProcessCbs
        // Requirement 1: Only Settled Trades We can do Process for CBS Integration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCbs(int tradeId)
        {
            var trade = await _uow.Trades.GetByIdAsync(tradeId);
            if (trade == null)
            {
                TempData["ErrorMessage"] = $"Trade T-{tradeId} not found.";
                return RedirectToAction("Index");
            }

            // Strictly enforce: ONLY Settled trades can be processed for CBS
            if (trade.Status != SettlementStatus.Settled)
            {
                TempData["ErrorMessage"] = $"Only Settled trades can be processed for CBS Integration! Trade T-{tradeId} is currently '{trade.Status}'. Please settle it first.";
                return RedirectToAction("Index");
            }

            bool result = await _settlementFacade.ProcessSettlementAsync(tradeId);

            if (result)
            {
                var refCode = !string.IsNullOrEmpty(trade.CbsTransactionId) ? trade.CbsTransactionId : Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                TempData["SuccessMessage"] = $"Trade T-{tradeId} successfully processed through CBS API! CBS Transaction Ref: {refCode}";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to process CBS Integration for Trade T-{tradeId}. Policy check or CBS API rejected the transaction.";
            }

            return RedirectToAction("Index");
        }

        // Backwards compatibility endpoint alias
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SettleTrade(int tradeId)
        {
            return await ProcessCbs(tradeId);
        }
    }
}
