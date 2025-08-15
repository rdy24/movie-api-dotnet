using Microsoft.EntityFrameworkCore;
using MovieApp.Data;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly CinemaDbContext _context;

        public TransactionRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.Schedule)
                        .ThenInclude(s => s.Studio)
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.Schedule)
                        .ThenInclude(s => s.Movie)
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.User)
                .Include(t => t.User)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.Schedule)
                        .ThenInclude(s => s.Studio)
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.Schedule)
                        .ThenInclude(s => s.Movie)
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.User)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Transaction> CreateAsync(Transaction transaction)
        {
            transaction.TransactionDate = DateTime.UtcNow;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            
            // Load related entities for return
            await _context.Entry(transaction)
                .Reference(t => t.Ticket)
                .LoadAsync();
            await _context.Entry(transaction.Ticket)
                .Reference(tk => tk.Schedule)
                .LoadAsync();
            await _context.Entry(transaction.Ticket.Schedule)
                .Reference(s => s.Studio)
                .LoadAsync();
            await _context.Entry(transaction.Ticket.Schedule)
                .Reference(s => s.Movie)
                .LoadAsync();
            await _context.Entry(transaction.Ticket)
                .Reference(tk => tk.User)
                .LoadAsync();
            await _context.Entry(transaction)
                .Reference(t => t.User)
                .LoadAsync();
                
            return transaction;
        }

        public async Task<Transaction?> UpdateAsync(int id, Transaction transaction)
        {
            var existingTransaction = await _context.Transactions.FindAsync(id);
            if (existingTransaction == null)
                return null;

            existingTransaction.TicketId = transaction.TicketId;
            existingTransaction.UserId = transaction.UserId;
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.PaymentMethod = transaction.PaymentMethod;
            existingTransaction.PaymentStatus = transaction.PaymentStatus;
            existingTransaction.PaymentReference = transaction.PaymentReference;

            await _context.SaveChangesAsync();
            
            // Load related entities for return
            await _context.Entry(existingTransaction)
                .Reference(t => t.Ticket)
                .LoadAsync();
            await _context.Entry(existingTransaction.Ticket)
                .Reference(tk => tk.Schedule)
                .LoadAsync();
            await _context.Entry(existingTransaction.Ticket.Schedule)
                .Reference(s => s.Studio)
                .LoadAsync();
            await _context.Entry(existingTransaction.Ticket.Schedule)
                .Reference(s => s.Movie)
                .LoadAsync();
            await _context.Entry(existingTransaction.Ticket)
                .Reference(tk => tk.User)
                .LoadAsync();
            await _context.Entry(existingTransaction)
                .Reference(t => t.User)
                .LoadAsync();
                
            return existingTransaction;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Transactions.AnyAsync(t => t.Id == id);
        }

        public async Task<bool> TicketExistsAsync(int ticketId)
        {
            return await _context.Tickets.AnyAsync(t => t.Id == ticketId);
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> TicketAlreadyPaidAsync(int ticketId, int? excludeTransactionId = null)
        {
            var query = _context.Transactions
                .Where(t => t.TicketId == ticketId && t.PaymentStatus == PaymentStatus.Success);

            if (excludeTransactionId.HasValue)
            {
                query = query.Where(t => t.Id != excludeTransactionId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId)
        {
            return await _context.Transactions
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.Schedule)
                        .ThenInclude(s => s.Studio)
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.Schedule)
                        .ThenInclude(s => s.Movie)
                .Include(t => t.User)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByTicketIdAsync(int ticketId)
        {
            return await _context.Transactions
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.Schedule)
                        .ThenInclude(s => s.Studio)
                .Include(t => t.Ticket)
                    .ThenInclude(tk => tk.Schedule)
                        .ThenInclude(s => s.Movie)
                .Include(t => t.User)
                .Where(t => t.TicketId == ticketId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}