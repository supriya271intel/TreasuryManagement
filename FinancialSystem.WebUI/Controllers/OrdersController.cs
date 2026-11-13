using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Infrastructure.Data;
using FinancialSystem.WebUI.Models;

namespace FinancialSystem.WebUI.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly ApplicationDbContext _db;
        private const int PageSize = 10;

        public OrdersController(IUnitOfWork uow, ApplicationDbContext db)
        {
            _uow = uow;
            _db  = db;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var allOrders  = (await _uow.Orders.GetAllAsync()).ToList();
            int total      = allOrders.Count;
            int totalPages = (int)System.Math.Ceiling(total / (double)PageSize);

            // Clamp page to valid range
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paged = allOrders
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var vm = new PagedOrdersViewModel
            {
                Orders        = paged,
                CurrentPage   = page,
                TotalPages    = totalPages == 0 ? 1 : totalPages,
                TotalRecords  = total,
                PageSize      = PageSize
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create(string type = "FD")
        {
            string selectedType = "FD";
            if (!string.IsNullOrEmpty(type))
            {
                var norm = type.Trim().ToLowerInvariant();
                if (norm == "gsec" || norm == "governmentsecurities" || norm == "government-securities") selectedType = "GSec";
                else if (norm == "mf" || norm == "mutualfund" || norm == "mutual-fund") selectedType = "MF";
                else selectedType = "FD";
            }

            var vm = new OrderCreateViewModel
            {
                InstrumentType = selectedType,
                GSecTradeDate  = DateTime.Today,
                FdDepositDate  = DateTime.Today
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel vm)
        {
            // Normalize instrument type
            var instType = (vm.InstrumentType ?? "FD").Trim();
            if (instType.Equals("gsec", StringComparison.OrdinalIgnoreCase) || instType.Contains("government", StringComparison.OrdinalIgnoreCase))
                instType = "GSec";
            else if (instType.Equals("mf", StringComparison.OrdinalIgnoreCase) || instType.Contains("mutual", StringComparison.OrdinalIgnoreCase))
                instType = "MF";
            else
                instType = "FD";

            vm.InstrumentType = instType;

            // ── Remove validation errors for fields of the OTHER instrument ──
            ClearIrrelevantValidation(vm.InstrumentType);

            if (!ModelState.IsValid)
                return View(vm);

            // Auto-populate Quantity and Price based on instrument if not explicitly set
            if (vm.InstrumentType == "FD")
            {
                vm.Quantity = vm.Quantity > 0 ? vm.Quantity : 1;
                vm.Price    = vm.FdPrincipal.HasValue && vm.FdPrincipal.Value > 0 ? vm.FdPrincipal.Value : (vm.Price > 0 ? vm.Price : 100000);
            }
            else if (vm.InstrumentType == "MF")
            {
                vm.Price    = vm.GSecDealPrice.HasValue && vm.GSecDealPrice.Value > 0 ? vm.GSecDealPrice.Value : (vm.Price > 0 ? vm.Price : 100);
                vm.Quantity = vm.Quantity > 0 ? vm.Quantity : (vm.GSecFaceValue.HasValue && vm.Price > 0 ? Math.Max(1, (int)(vm.GSecFaceValue.Value / vm.Price)) : 100);
            }
            else // GSec
            {
                vm.Quantity = vm.GSecFaceValue.HasValue && vm.GSecFaceValue.Value > 0 ? Math.Max(1, (int)(vm.GSecFaceValue.Value / 100)) : (vm.Quantity > 0 ? vm.Quantity : 100);
                vm.Price    = vm.GSecDealPrice.HasValue && vm.GSecDealPrice.Value > 0 ? vm.GSecDealPrice.Value : (vm.Price > 0 ? vm.Price : 100);
            }

            // ── Build the common Order entity ────────────────────────────────
            var order = new Order
            {
                Quantity  = vm.Quantity,
                Price     = vm.Price,
                Status    = OrderStatus.Pending,
                CreatedAt = DateTime.Now
            };

            switch (vm.InstrumentType)
            {
                case "GSec":
                    order.Instrument = new GSec
                    {
                        Name       = string.IsNullOrWhiteSpace(vm.GSecSecurityName) ? "Government Securities" : vm.GSecSecurityName.Trim(),
                        Isin       = string.IsNullOrWhiteSpace(vm.GSecIsin) ? "IN00" + DateTime.UtcNow.Ticks.ToString().Substring(10) : vm.GSecIsin.Trim().ToUpperInvariant(),
                        CouponRate = vm.GSecYield ?? 0
                    };
                    order.Isin           = order.Instrument.Isin;
                    order.IsinReportDate = vm.GSecTradeDate ?? DateTime.Today;

                    // Also create the full GSec deal ticket
                    var gsec = new GSecTrade
                    {
                        Isin         = order.Instrument.Isin,
                        ScriptCode   = (vm.GSecScriptCode ?? "").Trim(),
                        SecurityName = order.Instrument.Name,
                        MaturityDate = vm.GSecMaturityDate ?? DateTime.Today,
                        Action       = vm.GSecAction,
                        TradeDate    = vm.GSecTradeDate ?? DateTime.Today,
                        FaceValue    = vm.GSecFaceValue ?? 0,
                        Price        = vm.GSecDealPrice ?? 0,
                        Yield        = vm.GSecYield ?? 0,
                        DealValue    = vm.GSecDealValue ?? 0,
                        BrokenPeriod = vm.GSecBrokenPeriod ?? 0,
                        CreatedAt    = DateTime.UtcNow,
                        Status       = SettlementStatus.Pending
                    };
                    _db.GSecTrades.Add(gsec);
                    break;

                case "FD":
                    order.Instrument = new FixedDepositInstrument
                    {
                        Name = string.IsNullOrWhiteSpace(vm.FdName) ? "Fixed Deposit" : vm.FdName.Trim(),
                        Isin = string.IsNullOrWhiteSpace(vm.FdNumber) ? "FD-" + DateTime.Now.ToString("yyyyMMddHHmmss") : vm.FdNumber.Trim()
                    };
                    order.Isin             = order.Instrument.Isin;
                    order.FdMaturityDate   = vm.FdMaturityDate;
                    order.FdMaturityAmount = vm.FdPrincipal;
                    order.FdRoi            = vm.FdInterestRate;

                    // Also create the full Fixed Deposit record
                    int days = 0;
                    if (vm.FdMaturityDate.HasValue && vm.FdDepositDate.HasValue
                        && vm.FdMaturityDate > vm.FdDepositDate)
                    {
                        days = (vm.FdMaturityDate.Value - vm.FdDepositDate.Value).Days;
                    }

                    var fd = new FixedDeposit
                    {
                        FdName            = order.Instrument.Name,
                        FdNumber          = order.Instrument.Isin,
                        DepositDate       = vm.FdDepositDate ?? DateTime.Today,
                        MaturityDate      = vm.FdMaturityDate ?? DateTime.Today,
                        NumberOfDays      = vm.FdNumberOfDays ?? days,
                        Principal         = vm.FdPrincipal ?? 0,
                        InterestRate      = vm.FdInterestRate ?? 0,
                        InterestAmount    = vm.FdInterestAmount ?? 0,
                        InterestFrequency = vm.FdInterestFrequency,
                        Tds               = vm.FdTds ?? 0,
                        CreatedAt         = DateTime.UtcNow,
                        UpdatedAt         = DateTime.UtcNow
                    };
                    _db.FixedDeposits.Add(fd);
                    break;

                case "MF":
                default:
                    order.Instrument = new MutualFund
                    {
                        Name = string.IsNullOrWhiteSpace(vm.GSecSecurityName) ? "Mutual Fund" : vm.GSecSecurityName.Trim(),
                        Isin = string.IsNullOrWhiteSpace(vm.GSecIsin) ? "INF-" + DateTime.Now.ToString("yyyyMMddHHmmss") : vm.GSecIsin.Trim().ToUpperInvariant(),
                        Nav  = vm.GSecDealPrice ?? (vm.Price > 0 ? vm.Price : 100)
                    };
                    order.Isin           = order.Instrument.Isin;
                    order.IsinReportDate = vm.GSecTradeDate ?? DateTime.Today;
                    break;
            }

            await _uow.Orders.AddAsync(order);
            await _uow.CommitAsync();
            await _db.SaveChangesAsync();

            // Auto-queue corresponding trade in BackOffice for settlement
            var trade = new Trade
            {
                BuyOrder = order,
                TradePrice = order.Price,
                TradeQuantity = order.Quantity,
                Status = SettlementStatus.Pending,
                TradeDate = DateTime.Now
            };
            await _uow.Trades.AddAsync(trade);
            await _uow.CommitAsync();

            string displayLabel = vm.InstrumentType == "FD" ? "Fixed Deposit" : (vm.InstrumentType == "GSec" ? "Government Securities" : "Mutual Fund");
            TempData["SuccessMessage"] = $"Order created successfully ({displayLabel}) and queued in BackOffice Settlement.";
            return RedirectToAction("Index");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Strip validation errors for fields that belong to a different
        // instrument type so the form can submit cleanly.
        // ─────────────────────────────────────────────────────────────────────
        private void ClearIrrelevantValidation(string instrumentType)
        {
            var gsecKeys = ModelState.Keys.Where(k => k.StartsWith("GSec", StringComparison.OrdinalIgnoreCase)).ToList();
            var fdKeys   = ModelState.Keys.Where(k => k.StartsWith("Fd", StringComparison.OrdinalIgnoreCase)).ToList();

            if (instrumentType == "FD")
            {
                foreach (var k in gsecKeys) ModelState.Remove(k);
            }
            else
            {
                foreach (var k in fdKeys) ModelState.Remove(k);
            }
        }
    }
}
