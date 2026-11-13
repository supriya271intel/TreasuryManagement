using System;

namespace FinancialSystem.Domain.Entities
{
    /// <summary>
    /// Frequency at which FD interest is paid / compounded.
    /// </summary>
    public enum InterestFrequency
    {
        Monthly,
        Quarterly,
        HalfYearly,
        Annually,
        AtMaturity
    }

    /// <summary>
    /// Represents a Fixed Deposit (FD) instrument with all deal-level fields
    /// required for booking, TDS computation, and audit.
    /// </summary>
    public class FixedDeposit
    {
        public int Id { get; set; }

        // ── FD Identity ──────────────────────────────────────────────────
        /// <summary>Descriptive name / label for the FD (e.g. "SBI FD - Q1 2026")</summary>
        public string FdName { get; set; } = string.Empty;

        /// <summary>Unique FD receipt / certificate number issued by the bank</summary>
        public string FdNumber { get; set; } = string.Empty;

        // ── Dates ────────────────────────────────────────────────────────
        /// <summary>Date on which the FD was placed / opened</summary>
        public DateTime DepositDate { get; set; }

        /// <summary>Date on which the FD matures</summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>Computed tenure in calendar days (MaturityDate − DepositDate)</summary>
        public int NumberOfDays { get; set; }

        // ── Financials ───────────────────────────────────────────────────
        /// <summary>Original deposit / principal amount (₹)</summary>
        public decimal Principal { get; set; }

        /// <summary>Applicable interest rate per annum (%)</summary>
        public decimal InterestRate { get; set; }

        /// <summary>Total interest accrued / earned over the deposit tenure (₹)</summary>
        public decimal InterestAmount { get; set; }

        /// <summary>Frequency at which interest is paid or compounded</summary>
        public InterestFrequency InterestFrequency { get; set; }

        /// <summary>Tax Deducted at Source on interest income (₹)</summary>
        public decimal Tds { get; set; }

        // ── Audit ────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
