using MovieApp.Models;

namespace MovieApp.Interfaces
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction> CreateAsync(Transaction transaction);
        Task<Transaction?> UpdateAsync(int id, Transaction transaction);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TicketExistsAsync(int ticketId);
        Task<bool> UserExistsAsync(int userId);
        Task<bool> TicketAlreadyPaidAsync(int ticketId, int? excludeTransactionId = null);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Transaction>> GetByTicketIdAsync(int ticketId);
    }
}