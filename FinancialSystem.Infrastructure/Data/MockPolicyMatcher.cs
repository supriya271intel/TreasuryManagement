using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Infrastructure.Data
{
    public class MockPolicyMatcher : IPolicyMatcher
    {
        public bool IsMatchSuccessful(Trade trade)
        {
            // Just mock it to true for now
            return true;
        }
    }
}
