using System;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Application.Factories
{

public interface ITradeProcessor
{
    void ProcessTrade();
}

public interface ITradeProcessorFactory
{
    ITradeProcessor CreateProcessor(Instrument instrument);
}

public class MutualFundProcessor : ITradeProcessor
{
    public void ProcessTrade() { /* MF specific logic */ }
}

public class GSecProcessor : ITradeProcessor
{
    public void ProcessTrade() { /* GSec specific logic */ }
}

public class TradeProcessorFactory : ITradeProcessorFactory
{
    public ITradeProcessor CreateProcessor(Instrument instrument)
    {
        return instrument switch
        {
            MutualFund _ => new MutualFundProcessor(),
            GSec _ => new GSecProcessor(),
            _ => throw new NotSupportedException("Instrument not supported")
        };
    }
}

}
