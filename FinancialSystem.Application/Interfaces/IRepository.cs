using System.Threading.Tasks;
using System.Collections.Generic;

namespace FinancialSystem.Application.Interfaces
{

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

}
