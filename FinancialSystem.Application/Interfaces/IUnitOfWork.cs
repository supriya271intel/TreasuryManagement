using System;
using System.Threading.Tasks;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Application.Interfaces
{

public interface IUnitOfWork : IDisposable
{
    IRepository<Order> Orders { get; }
    IRepository<Trade> Trades { get; }
    IRepository<AppUser> Users { get; }
    Task<int> CommitAsync();
}

}
