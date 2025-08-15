using MovieApp.Models;

namespace MovieApp.Interfaces
{
    public interface IStudioRepository
    {
        Task<IEnumerable<Studio>> GetAllAsync();
        Task<Studio?> GetByIdAsync(int id);
        Task<Studio> CreateAsync(Studio studio);
        Task<Studio?> UpdateAsync(int id, Studio studio);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}