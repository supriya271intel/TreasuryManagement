using System;

namespace FinancialSystem.Domain.Entities
{

public enum SettlementStatus
{
    Pending,
    Settled,
    Failed
}

public class Trade
{
    public int Id { get; set; }
    public Order BuyOrder { get; set; }
    public Order SellOrder { get; set; }
    public decimal TradePrice { get; set; }
    public int TradeQuantity { get; set; }
    public SettlementStatus Status { get; set; }
    public DateTime TradeDate { get; set; }

    // CBS Integration fields
    public bool IsCbsProcessed { get; set; }
    public string? CbsTransactionId { get; set; }
    public DateTime? CbsProcessedAt { get; set; }
}

}
