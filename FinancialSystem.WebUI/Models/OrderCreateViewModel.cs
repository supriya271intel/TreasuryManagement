using System;
using System.ComponentModel.DataAnnotations;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.WebUI.Models
{
    /// <summary>
    /// Composite view-model for Orders/Create.
    /// Contains common order fields plus instrument-specific sub-forms
    /// for GSec and Fixed Deposit. The <see cref="InstrumentType"/> value
    /// drives which panel is displayed (and validated server-side).
    /// </summary>
    public class OrderCreateViewModel
    {
        // ── Instrument selector ──────────────────────────────────────────
        [Required]
        [Display(Name = "Instrument Type")]
        public string InstrumentType { get; set; } = "GSec";

        // ── Common Order fields (populated from instrument-specific values) ──
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Price (₹)")]
        public decimal Price { get; set; } = 0;

        // ══════════════════════════════════════════════════════════════════
        // GSec-specific fields (shown when InstrumentType == "GSec")
        // ══════════════════════════════════════════════════════════════════

        [Display(Name = "ISIN")]
        public string? GSecIsin { get; set; }

        [Display(Name = "Script Code")]
        public string? GSecScriptCode { get; set; }

        [Display(Name = "Security Name")]
        public string? GSecSecurityName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Maturity Date")]
        public DateTime? GSecMaturityDate { get; set; }

        [Display(Name = "Action")]
        public GSecAction GSecAction { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Trade Date")]
        public DateTime? GSecTradeDate { get; set; }

        [Display(Name = "Face Value (₹)")]
        public decimal? GSecFaceValue { get; set; }

        [Display(Name = "Deal Price (per ₹100 FV)")]
        public decimal? GSecDealPrice { get; set; }

        [Display(Name = "Yield (%)")]
        public decimal? GSecYield { get; set; }

        [Display(Name = "Deal Value (₹)")]
        public decimal? GSecDealValue { get; set; }

        [Display(Name = "Broken Period Interest (₹)")]
        public decimal? GSecBrokenPeriod { get; set; }

        // ══════════════════════════════════════════════════════════════════
        // Fixed Deposit-specific fields (shown when InstrumentType == "FD")
        // ══════════════════════════════════════════════════════════════════

        [Display(Name = "FD Name")]
        public string? FdName { get; set; }

        [Display(Name = "FD Number")]
        public string? FdNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Deposit Date")]
        public DateTime? FdDepositDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Maturity Date")]
        public DateTime? FdMaturityDate { get; set; }

        [Display(Name = "No. of Days")]
        public int? FdNumberOfDays { get; set; }

        [Display(Name = "Principal (₹)")]
        public decimal? FdPrincipal { get; set; }

        [Display(Name = "Interest Rate (% p.a.)")]
        public decimal? FdInterestRate { get; set; }

        [Display(Name = "Interest Amount (₹)")]
        public decimal? FdInterestAmount { get; set; }

        [Display(Name = "Interest Frequency")]
        public InterestFrequency FdInterestFrequency { get; set; }

        [Display(Name = "TDS (₹)")]
        public decimal? FdTds { get; set; }
    }
}
