using Microsoft.EntityFrameworkCore;
using MovieApp.Data;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly CinemaDbContext _context;

        public TicketRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _context.Tickets
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Studio)
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Studio)
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            ticket.BookedAt = DateTime.UtcNow;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            
            // Load related entities for return
            await _context.Entry(ticket)
                .Reference(t => t.Schedule)
                .LoadAsync();
            await _context.Entry(ticket.Schedule)
                .Reference(s => s.Studio)
                .LoadAsync();
            await _context.Entry(ticket.Schedule)
                .Reference(s => s.Movie)
                .LoadAsync();
            await _context.Entry(ticket)
                .Reference(t => t.User)
                .LoadAsync();
                
            return ticket;
        }

        public async Task<Ticket?> UpdateAsync(int id, Ticket ticket)
        {
            var existingTicket = await _context.Tickets.FindAsync(id);
            if (existingTicket == null)
                return null;

            existingTicket.ScheduleId = ticket.ScheduleId;
            existingTicket.UserId = ticket.UserId;
            existingTicket.SeatNumber = ticket.SeatNumber;
            existingTicket.Status = ticket.Status;

            await _context.SaveChangesAsync();
            
            // Load related entities for return
            await _context.Entry(existingTicket)
                .Reference(t => t.Schedule)
                .LoadAsync();
            await _context.Entry(existingTicket.Schedule)
                .Reference(s => s.Studio)
                .LoadAsync();
            await _context.Entry(existingTicket.Schedule)
                .Reference(s => s.Movie)
                .LoadAsync();
            await _context.Entry(existingTicket)
                .Reference(t => t.User)
                .LoadAsync();
                
            return existingTicket;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Tickets.AnyAsync(t => t.Id == id);
        }

        public async Task<bool> ScheduleExistsAsync(int scheduleId)
        {
            return await _context.Schedules.AnyAsync(s => s.Id == scheduleId);
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> SeatAvailableAsync(int scheduleId, string seatNumber, int? excludeTicketId = null)
        {
            var query = _context.Tickets
                .Where(t => t.ScheduleId == scheduleId && 
                           t.SeatNumber == seatNumber && 
                           t.Status != TicketStatus.Cancelled);

            if (excludeTicketId.HasValue)
            {
                query = query.Where(t => t.Id != excludeTicketId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}