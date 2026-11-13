using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Infrastructure.Data
{

    // Removed MockRepository

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IRepository<Order> Orders { get; }
        public IRepository<Trade> Trades { get; }
        public IRepository<AppUser> Users { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Orders = new Repository<Order>(_context);
            Trades = new Repository<Trade>(_context);
            Users = new Repository<AppUser>(_context);
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
