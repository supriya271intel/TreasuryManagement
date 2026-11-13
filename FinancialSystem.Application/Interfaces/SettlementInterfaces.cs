using System.Threading.Tasks;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Application.Interfaces
{

public interface IPolicyMatcher
{
    bool IsMatchSuccessful(Trade trade);
}

public interface ICbsApiService
{
    Task<(bool IsSuccess, string TransactionId)> PostSettlementAsync(Trade trade);
}

public interface ISettlementFacade
{
    Task<bool> ProcessSettlementAsync(int tradeId);
}

}
