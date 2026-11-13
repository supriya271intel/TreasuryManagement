using System;

namespace FinancialSystem.Domain.Entities
{
    /// <summary>
    /// Represents a Government Security (GSec) trade deal ticket with all
    /// standard RBI / SGL fields required for settlement and reporting.
    /// </summary>
    public enum GSecAction
    {
        Buy,
        Sell
    }

    public class GSecTrade
    {
        public int Id { get; set; }

        // ── Security Identification ──────────────────────────────────────
        /// <summary>International Securities Identification Number (e.g. IN0020200001)</summary>
        public string Isin { get; set; } = string.Empty;

        /// <summary>Exchange / CBS script code for the security</summary>
        public string ScriptCode { get; set; } = string.Empty;

        /// <summary>Full name / description of the Government Security</summary>
        public string SecurityName { get; set; } = string.Empty;

        /// <summary>Date on which the security matures / redeems at par</summary>
        public DateTime MaturityDate { get; set; }

        // ── Deal Parameters ──────────────────────────────────────────────
        /// <summary>Buy or Sell direction of the deal</summary>
        public GSecAction Action { get; set; }

        /// <summary>Date on which the deal / trade was executed</summary>
        public DateTime TradeDate { get; set; }

        /// <summary>Face (nominal) value of securities transacted (₹)</summary>
        public decimal FaceValue { get; set; }

        /// <summary>Clean price per ₹100 face value at which the deal was struck</summary>
        public decimal Price { get; set; }

        /// <summary>Yield to Maturity (YTM) at deal price, expressed as a percentage</summary>
        public decimal Yield { get; set; }

        /// <summary>Total consideration = (Price / 100) × FaceValue + BrokenPeriodInterest (₹)</summary>
        public decimal DealValue { get; set; }

        /// <summary>Accrued / broken-period interest component of the deal consideration (₹)</summary>
        public decimal BrokenPeriod { get; set; }

        // ── Audit ────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public SettlementStatus Status { get; set; } = SettlementStatus.Pending;
    }
}
