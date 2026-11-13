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
    public class FixedDepositController : Controller
    {
        private readonly ApplicationDbContext _db;

        public FixedDepositController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /FixedDeposit
        public async Task<IActionResult> Index()
        {
            var fds = await _db.FixedDeposits
                               .OrderByDescending(f => f.DepositDate)
                               .ToListAsync();
            return View(fds);
        }

        // GET: /FixedDeposit/Create
        public IActionResult Create()
        {
            return RedirectToAction("Create", "Orders", new { type = "FD" });
        }

        // POST: /FixedDeposit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FixedDepositViewModel vm)
        {
            // Auto-compute NumberOfDays if not manually supplied
            if (vm.NumberOfDays <= 0 && vm.MaturityDate > vm.DepositDate)
                vm.NumberOfDays = (vm.MaturityDate - vm.DepositDate).Days;

            if (!ModelState.IsValid)
                return View(vm);

            var entity = MapToEntity(vm);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            _db.FixedDeposits.Add(entity);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Fixed Deposit \"{entity.FdName}\" (#{entity.FdNumber}) created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /FixedDeposit/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _db.FixedDeposits.FindAsync(id);
            if (entity == null) return NotFound();
            return View(MapToVm(entity));
        }

        // POST: /FixedDeposit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FixedDepositViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (vm.NumberOfDays <= 0 && vm.MaturityDate > vm.DepositDate)
                vm.NumberOfDays = (vm.MaturityDate - vm.DepositDate).Days;

            if (!ModelState.IsValid) return View(vm);

            var entity = await _db.FixedDeposits.FindAsync(id);
            if (entity == null) return NotFound();

            ApplyVmToEntity(vm, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Fixed Deposit \"{entity.FdName}\" (#{entity.FdNumber}) updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /FixedDeposit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _db.FixedDeposits.FindAsync(id);
            if (entity != null)
            {
                _db.FixedDeposits.Remove(entity);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Fixed Deposit #{id} deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private static FixedDeposit MapToEntity(FixedDepositViewModel vm) => new()
        {
            Id                = vm.Id,
            FdName            = vm.FdName.Trim(),
            FdNumber          = vm.FdNumber.Trim(),
            DepositDate       = vm.DepositDate,
            MaturityDate      = vm.MaturityDate,
            NumberOfDays      = vm.NumberOfDays,
            Principal         = vm.Principal,
            InterestRate      = vm.InterestRate,
            InterestAmount    = vm.InterestAmount,
            InterestFrequency = vm.InterestFrequency,
            Tds               = vm.Tds
        };

        private static void ApplyVmToEntity(FixedDepositViewModel vm, FixedDeposit e)
        {
            e.FdName            = vm.FdName.Trim();
            e.FdNumber          = vm.FdNumber.Trim();
            e.DepositDate       = vm.DepositDate;
            e.MaturityDate      = vm.MaturityDate;
            e.NumberOfDays      = vm.NumberOfDays;
            e.Principal         = vm.Principal;
            e.InterestRate      = vm.InterestRate;
            e.InterestAmount    = vm.InterestAmount;
            e.InterestFrequency = vm.InterestFrequency;
            e.Tds               = vm.Tds;
        }

        private static FixedDepositViewModel MapToVm(FixedDeposit e) => new()
        {
            Id                = e.Id,
            FdName            = e.FdName,
            FdNumber          = e.FdNumber,
            DepositDate       = e.DepositDate,
            MaturityDate      = e.MaturityDate,
            NumberOfDays      = e.NumberOfDays,
            Principal         = e.Principal,
            InterestRate      = e.InterestRate,
            InterestAmount    = e.InterestAmount,
            InterestFrequency = e.InterestFrequency,
            Tds               = e.Tds,
            CreatedAt         = e.CreatedAt,
            UpdatedAt         = e.UpdatedAt
        };
    }
}
