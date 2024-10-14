using Merken.Core.Models.Abstractions;

namespace Merken.Core.Services.Abstractions;

public interface IStorageService<T>
    where T : DbModel
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<T?> AddAsync(T model);
    Task<T?> UpdateAsync(T model);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> Sync();
}