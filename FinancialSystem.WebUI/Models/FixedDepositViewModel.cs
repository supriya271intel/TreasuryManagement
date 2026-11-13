using System;
using System.ComponentModel.DataAnnotations;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.WebUI.Models
{
    /// <summary>
    /// View-model for the Fixed Deposit create / edit form.
    /// </summary>
    public class FixedDepositViewModel
    {
        public int Id { get; set; }

        // ── FD Identity ──────────────────────────────────────────────────
        [Required]
        [Display(Name = "FD Name")]
        [StringLength(150)]
        public string FdName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "FD Number")]
        [StringLength(50)]
        public string FdNumber { get; set; } = string.Empty;

        // ── Dates ────────────────────────────────────────────────────────
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Deposit Date")]
        public DateTime DepositDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Maturity Date")]
        public DateTime MaturityDate { get; set; }

        [Display(Name = "No. of Days")]
        public int NumberOfDays { get; set; }

        // ── Financials ───────────────────────────────────────────────────
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal must be positive.")]
        [Display(Name = "Principal (₹)")]
        public decimal Principal { get; set; }

        [Required]
        [Range(0.0001, 100, ErrorMessage = "Interest rate must be between 0% and 100%.")]
        [Display(Name = "Interest Rate (% p.a.)")]
        public decimal InterestRate { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Interest amount must be non-negative.")]
        [Display(Name = "Interest Amount (₹)")]
        public decimal InterestAmount { get; set; }

        [Required]
        [Display(Name = "Interest Frequency")]
        public InterestFrequency InterestFrequency { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "TDS must be non-negative.")]
        [Display(Name = "TDS (₹)")]
        public decimal Tds { get; set; }

        // ── Audit (display only) ─────────────────────────────────────────
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }
    }
}
