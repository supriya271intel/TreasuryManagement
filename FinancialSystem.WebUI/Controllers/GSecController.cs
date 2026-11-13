using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Infrastructure.Data;
using FinancialSystem.WebUI.Models;

namespace FinancialSystem.WebUI.Controllers
{
    public class GSecController : Controller
    {
        private readonly ApplicationDbContext _db;

        public GSecController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /GSec
        public async Task<IActionResult> Index()
        {
            var trades = await _db.GSecTrades
                                  .OrderByDescending(t => t.TradeDate)
                                  .ToListAsync();
            return View(trades);
        }

        // GET: /GSec/Create
        public IActionResult Create()
        {
            return RedirectToAction("Create", "Orders", new { type = "GSec" });
        }

        // POST: /GSec/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GSecTradeViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var entity = new GSecTrade
            {
                Isin         = vm.Isin.Trim().ToUpperInvariant(),
                ScriptCode   = vm.ScriptCode.Trim(),
                SecurityName = vm.SecurityName.Trim(),
                MaturityDate = vm.MaturityDate,
                Action       = vm.Action,
                TradeDate    = vm.TradeDate,
                FaceValue    = vm.FaceValue,
                Price        = vm.Price,
                Yield        = vm.Yield,
                DealValue    = vm.DealValue,
                BrokenPeriod = vm.BrokenPeriod,
                CreatedAt    = DateTime.UtcNow,
                Status       = SettlementStatus.Pending
            };

            _db.GSecTrades.Add(entity);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"GSec deal for {entity.SecurityName} ({entity.Isin}) created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /GSec/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _db.GSecTrades.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToVm(entity);
            return View(vm);
        }

        // POST: /GSec/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GSecTradeViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var entity = await _db.GSecTrades.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Isin         = vm.Isin.Trim().ToUpperInvariant();
            entity.ScriptCode   = vm.ScriptCode.Trim();
            entity.SecurityName = vm.SecurityName.Trim();
            entity.MaturityDate = vm.MaturityDate;
            entity.Action       = vm.Action;
            entity.TradeDate    = vm.TradeDate;
            entity.FaceValue    = vm.FaceValue;
            entity.Price        = vm.Price;
            entity.Yield        = vm.Yield;
            entity.DealValue    = vm.DealValue;
            entity.BrokenPeriod = vm.BrokenPeriod;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"GSec deal #{id} updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /GSec/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _db.GSecTrades.FindAsync(id);
            if (entity != null)
            {
                _db.GSecTrades.Remove(entity);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"GSec deal #{id} deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── Helper ────────────────────────────────────────────────────────
        private static GSecTradeViewModel MapToVm(GSecTrade e) => new()
        {
            Id           = e.Id,
            Isin         = e.Isin,
            ScriptCode   = e.ScriptCode,
            SecurityName = e.SecurityName,
            MaturityDate = e.MaturityDate,
            Action       = e.Action,
            TradeDate    = e.TradeDate,
            FaceValue    = e.FaceValue,
            Price        = e.Price,
            Yield        = e.Yield,
            DealValue    = e.DealValue,
            BrokenPeriod = e.BrokenPeriod,
            Status       = e.Status,
            CreatedAt    = e.CreatedAt
        };
    }
}
