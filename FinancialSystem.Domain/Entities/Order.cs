using System;

namespace FinancialSystem.Domain.Entities
{

public enum OrderStatus
{
    Pending,
    Matched,
    Rejected
}

public class Order
{
    public int Id { get; set; }
    public Instrument Instrument { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // GSec-specific fields
    public string? Isin { get; set; }
    public DateTime? IsinReportDate { get; set; }

    // FD-specific fields
    public DateTime? FdMaturityDate { get; set; }
    public decimal? FdMaturityAmount { get; set; }
    public decimal? FdRoi { get; set; }
}

}
