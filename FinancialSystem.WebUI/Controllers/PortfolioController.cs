using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Entities.Portfolio;

namespace FinancialSystem.WebUI.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly IUnitOfWork _uow;

        public PortfolioController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Portfolio
        public async Task<IActionResult> Index()
        {
            var orders = await _uow.Orders.GetAllAsync();

            // Build the Composite tree using the Composite Pattern
            var root = new AssetPortfolio { PortfolioName = "Total Portfolio" };

            // Requirement: For GSec Asset class would be Government Securities
            var gsecCategory = new AssetPortfolio { PortfolioName = "Government Securities" };
            var fdCategory   = new AssetPortfolio { PortfolioName = "Fixed Deposits" };
            var equities     = new AssetPortfolio { PortfolioName = "Equities & Mutual Funds" };

            foreach (var order in orders)
            {
                var inst = order.Instrument;
                string instType = "Mutual Fund";
                if (inst is FixedDepositInstrument || order.FdMaturityDate.HasValue || (inst?.Name != null && (inst.Name.Contains("Fixed Deposit", StringComparison.OrdinalIgnoreCase) || inst.Name.Contains(" Term Deposit", StringComparison.OrdinalIgnoreCase) || inst.Name.Contains(" FD", StringComparison.OrdinalIgnoreCase))) || (inst?.Isin != null && inst.Isin.StartsWith("FD-", StringComparison.OrdinalIgnoreCase)))
                {
                    instType = "Fixed Deposit";
                }
                else if (inst is GSec || (inst?.Name != null && (inst.Name.Contains("GOI", StringComparison.OrdinalIgnoreCase) || inst.Name.Contains("G-Sec", StringComparison.OrdinalIgnoreCase))))
                {
                    instType = "Government Securities";
                }

                var asset = new SingleAsset
                {
                    Name = inst?.Name ?? "Unknown",
                    Isin = inst?.Isin ?? order.Isin ?? "-",
                    InstrumentType = instType,
                    Value = order.Price,
                    Quantity = order.Quantity
                };

                if (instType == "Government Securities")
                    gsecCategory.Add(asset);
                else if (instType == "Fixed Deposit")
                    fdCategory.Add(asset);
                else
                    equities.Add(asset);
            }

            if (gsecCategory.Children.Count > 0) root.Add(gsecCategory);
            if (fdCategory.Children.Count > 0) root.Add(fdCategory);
            if (equities.Children.Count > 0) root.Add(equities);

            return View(root);
        }

        // GET: /Portfolio/Details
        // Portfolio Details Page Powered by Angular and AG Grid
        [HttpGet]
        public IActionResult Details()
        {
            return View();
        }

        // GET: /Portfolio/GetPortfolioData
        // JSON API endpoint supplying data to the Angular AG Grid (Latest first)
        [HttpGet]
        public async Task<IActionResult> GetPortfolioData()
        {
            var orders = (await _uow.Orders.GetAllAsync()).OrderByDescending(o => o.CreatedAt).ToList();

            var data = orders.Select(order =>
            {
                var inst = order.Instrument;
                string instType = "Mutual Fund";
                string assetClass = "Equities & Funds";
                string specs = "";
                decimal? roiOrRate = null;

                if (inst is FixedDepositInstrument || order.FdMaturityDate.HasValue || (inst?.Name != null && (inst.Name.Contains("Fixed Deposit", StringComparison.OrdinalIgnoreCase) || inst.Name.Contains(" Term Deposit", StringComparison.OrdinalIgnoreCase) || inst.Name.Contains(" FD", StringComparison.OrdinalIgnoreCase))) || (inst?.Isin != null && inst.Isin.StartsWith("FD-", StringComparison.OrdinalIgnoreCase)))
                {
                    instType = "Fixed Deposit";
                    assetClass = "Fixed Deposits";
                    roiOrRate = order.FdRoi ?? 7.25m;
                    specs = $"ROI: {roiOrRate:F2}% | Maturity: {order.FdMaturityDate?.ToString("dd MMM yyyy") ?? "365 Days"}";
                }
                else if (inst is GSec gsec || (inst?.Name != null && (inst.Name.Contains("GOI", StringComparison.OrdinalIgnoreCase) || inst.Name.Contains("G-Sec", StringComparison.OrdinalIgnoreCase))))
                {
                    instType = "Government Securities";
                    assetClass = "Government Securities";
                    roiOrRate = (inst as GSec)?.CouponRate ?? 7.10m;
                    specs = $"Coupon: {roiOrRate:F2}% | Issue: {order.IsinReportDate?.ToString("dd MMM yyyy") ?? "Active Central Govt"}";
                }
                else
                {
                    instType = "Mutual Fund";
                    assetClass = "Equities & Mutual Funds";
                    roiOrRate = (inst as MutualFund)?.Nav ?? 45.20m;
                    specs = $"NAV: ₹{roiOrRate:F2}";
                }

                return new
                {
                    id = order.Id,
                    orderCode = $"ORD-{order.Id:D4}",
                    name = inst?.Name ?? "Financial Asset Deal",
                    isin = inst?.Isin ?? order.Isin ?? "N/A",
                    instrumentType = instType,
                    assetClass = assetClass,
                    price = order.Price,
                    quantity = order.Quantity,
                    totalValue = order.Price * order.Quantity,
                    status = order.Status.ToString(),
                    dateIso = order.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    dateDisplay = order.CreatedAt.ToString("dd MMM yyyy, HH:mm"),
                    dateOnly = order.CreatedAt.ToString("yyyy-MM-dd"),
                    maturityDate = order.FdMaturityDate?.ToString("dd MMM yyyy") ?? "-",
                    specifications = specs,
                    rateOrNav = roiOrRate
                };
            }).ToList();

            return Json(data);
        }

        // GET: /Portfolio/GetInstrumentWiseData
        // Requirement: Show instrument wise details with latest records up at top
        [HttpGet]
        public async Task<IActionResult> GetInstrumentWiseData()
        {
            var orders = await _uow.Orders.GetAllAsync();

            var grouped = orders
                .GroupBy(o => new
                {
                    Name = o.Instrument?.Name ?? "Financial Asset Deal",
                    Isin = o.Instrument?.Isin ?? o.Isin ?? "N/A"
                })
                .Select(g =>
                {
                    var latestOrder = g.OrderByDescending(o => o.CreatedAt).First();
                    var inst = latestOrder.Instrument;
                    string instType = "Mutual Fund";
                    string assetClass = "Equities & Funds";
                    decimal? roiOrRate = null;

                    if (inst is FixedDepositInstrument || latestOrder.FdMaturityDate.HasValue || (inst?.Name != null && (inst.Name.Contains("Fixed Deposit", StringComparison.OrdinalIgnoreCase) || inst.Name.Contains(" Term Deposit", StringComparison.OrdinalIgnoreCase) || inst.Name.Contains(" FD", StringComparison.OrdinalIgnoreCase))) || (inst?.Isin != null && inst.Isin.StartsWith("FD-", StringComparison.OrdinalIgnoreCase)))
                    {
                        instType = "Fixed Deposit";
                        assetClass = "Fixed Deposits";
                        roiOrRate = latestOrder.FdRoi ?? 7.25m;
                    }
                    else if (inst is GSec gsec || (inst?.Name != null && (inst.Name.Contains("GOI", StringComparison.OrdinalIgnoreCase) || inst.Name.Contains("G-Sec", StringComparison.OrdinalIgnoreCase))))
                    {
                        instType = "Government Securities";
                        assetClass = "Government Securities";
                        roiOrRate = (inst as GSec)?.CouponRate ?? 7.10m;
                    }
                    else
                    {
                        instType = "Mutual Fund";
                        assetClass = "Equities & Mutual Funds";
                        roiOrRate = (inst as MutualFund)?.Nav ?? 45.20m;
                    }

                    int totalQty = g.Sum(o => o.Quantity);
                    decimal totalVal = g.Sum(o => o.Price * o.Quantity);
                    decimal avgPrice = totalQty > 0 ? totalVal / totalQty : 0;

                    return new
                    {
                        instrumentName = g.Key.Name,
                        isin = g.Key.Isin,
                        instrumentType = instType,
                        assetClass = assetClass,
                        totalQuantity = totalQty,
                        totalValue = totalVal,
                        averagePrice = avgPrice,
                        latestPrice = latestOrder.Price,
                        latestDateIso = latestOrder.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        latestDateDisplay = latestOrder.CreatedAt.ToString("dd MMM yyyy, HH:mm"),
                        rateOrNav = roiOrRate,
                        orderCount = g.Count(),
                        orders = g.OrderByDescending(o => o.CreatedAt).Select(o => new
                        {
                            id = o.Id,
                            orderCode = $"ORD-{o.Id:D4}",
                            price = o.Price,
                            quantity = o.Quantity,
                            totalValue = o.Price * o.Quantity,
                            status = o.Status.ToString(),
                            dateDisplay = o.CreatedAt.ToString("dd MMM yyyy, HH:mm"),
                            dateIso = o.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
                        }).ToList()
                    };
                })
                .OrderByDescending(x => x.latestDateIso) // Latest show at up!
                .ToList();

            return Json(grouped);
        }
    }
}
