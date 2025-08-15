using MovieApp.Models;

namespace MovieApp.Interfaces
{
    public interface IScheduleRepository
    {
        Task<IEnumerable<Schedule>> GetAllAsync();
        Task<Schedule?> GetByIdAsync(int id);
        Task<Schedule> CreateAsync(Schedule schedule);
        Task<Schedule?> UpdateAsync(int id, Schedule schedule);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> StudioExistsAsync(int studioId);
        Task<bool> MovieExistsAsync(int movieId);
    }
}