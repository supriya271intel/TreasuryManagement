using System;
using System.ComponentModel.DataAnnotations;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.WebUI.Models
{
    /// <summary>
    /// View-model used by the GSC Deal Ticket create / edit form.
    /// </summary>
    public class GSecTradeViewModel
    {
        public int Id { get; set; }

        // ── Security Identification ───────────────────────────────────────
        [Required]
        [StringLength(12, MinimumLength = 12)]
        [Display(Name = "ISIN")]
        public string Isin { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Script Code")]
        public string ScriptCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Security Name")]
        public string SecurityName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Maturity Date")]
        public DateTime MaturityDate { get; set; }

        // ── Deal Parameters ───────────────────────────────────────────────
        [Required]
        [Display(Name = "Action")]
        public GSecAction Action { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Trade Date")]
        public DateTime TradeDate { get; set; } = DateTime.Today;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Face Value must be positive.")]
        [Display(Name = "Face Value (₹)")]
        public decimal FaceValue { get; set; }

        [Required]
        [Range(0.0001, 200, ErrorMessage = "Price must be between 0 and 200.")]
        [Display(Name = "Price (per ₹100 FV)")]
        public decimal Price { get; set; }

        [Required]
        [Range(0.0001, 100, ErrorMessage = "Yield must be between 0% and 100%.")]
        [Display(Name = "Yield (%)")]
        public decimal Yield { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Deal Value must be non-negative.")]
        [Display(Name = "Deal Value (₹)")]
        public decimal DealValue { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Broken Period must be non-negative.")]
        [Display(Name = "Broken Period Interest (₹)")]
        public decimal BrokenPeriod { get; set; }

        // ── Status (display only on list) ─────────────────────────────────
        public SettlementStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
