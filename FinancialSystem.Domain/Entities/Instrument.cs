namespace FinancialSystem.Domain.Entities
{

public abstract class Instrument
{
    public int Id { get; set; }
    public string Isin { get; set; }
    public string Name { get; set; }
}

public class MutualFund : Instrument
{
    public decimal Nav { get; set; }
}

public class GSec : Instrument
{
    public decimal CouponRate { get; set; }
}

public class FixedDepositInstrument : Instrument
{
}

}
