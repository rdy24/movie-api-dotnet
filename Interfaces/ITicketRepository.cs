using MovieApp.Models;

namespace MovieApp.Interfaces
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket> CreateAsync(Ticket ticket);
        Task<Ticket?> UpdateAsync(int id, Ticket ticket);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ScheduleExistsAsync(int scheduleId);
        Task<bool> UserExistsAsync(int userId);
        Task<bool> SeatAvailableAsync(int scheduleId, string seatNumber, int? excludeTicketId = null);
    }
}