using System.Threading.Tasks;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Application.Services
{

public class SettlementFacade : ISettlementFacade
{
    private readonly IUnitOfWork _uow;
    private readonly IPolicyMatcher _matcher;
    private readonly ICbsApiService _cbsApi;

    public SettlementFacade(IUnitOfWork uow, IPolicyMatcher matcher, ICbsApiService cbsApi)
    {
        _uow = uow;
        _matcher = matcher;
        _cbsApi = cbsApi;
    }

    public async Task<bool> ProcessSettlementAsync(int tradeId)
    {
        // 1. Fetch Trade
        var trade = await _uow.Trades.GetByIdAsync(tradeId);
        if (trade == null) return false;
        
        // Requirement 1: Only Settled Trades can do Process for CBS Integration
        if (trade.Status != SettlementStatus.Settled) return false;

        // 2. Policy & Matching
        if (!_matcher.IsMatchSuccessful(trade)) return false;

        // 3. CBS Integration
        var cbsResponse = await _cbsApi.PostSettlementAsync(trade);
        if (!cbsResponse.IsSuccess) return false;

        // 4. Update local DB with CBS integration record
        trade.IsCbsProcessed = true;
        trade.CbsTransactionId = cbsResponse.TransactionId;
        trade.CbsProcessedAt = System.DateTime.Now;
        await _uow.CommitAsync();

        return true;
    }
}

}
