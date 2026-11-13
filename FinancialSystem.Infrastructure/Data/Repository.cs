using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinancialSystem.Application.Interfaces;

namespace FinancialSystem.Infrastructure.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Specifically for Orders and Trades to eagerly load related entities
            // Wait, standard GetAllAsync might need includes, but let's keep it simple or check type
            if (typeof(T) == typeof(FinancialSystem.Domain.Entities.Order))
            {
                return (IEnumerable<T>)await _context.Orders.Include(o => o.Instrument).ToListAsync();
            }
            if (typeof(T) == typeof(FinancialSystem.Domain.Entities.Trade))
            {
                return (IEnumerable<T>)await _context.Trades
                    .Include(t => t.BuyOrder).ThenInclude(o => o.Instrument)
                    .Include(t => t.SellOrder).ThenInclude(o => o.Instrument)
                    .ToListAsync();
            }

            return await _dbSet.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
