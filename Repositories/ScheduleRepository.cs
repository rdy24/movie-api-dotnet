using Microsoft.EntityFrameworkCore;
using MovieApp.Data;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly CinemaDbContext _context;

        public ScheduleRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Schedule>> GetAllAsync()
        {
            return await _context.Schedules
                .Include(s => s.Studio)
                .Include(s => s.Movie)
                .ToListAsync();
        }

        public async Task<Schedule?> GetByIdAsync(int id)
        {
            return await _context.Schedules
                .Include(s => s.Studio)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Schedule> CreateAsync(Schedule schedule)
        {
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            
            // Load related entities for return
            await _context.Entry(schedule)
                .Reference(s => s.Studio)
                .LoadAsync();
            await _context.Entry(schedule)
                .Reference(s => s.Movie)
                .LoadAsync();
                
            return schedule;
        }

        public async Task<Schedule?> UpdateAsync(int id, Schedule schedule)
        {
            var existingSchedule = await _context.Schedules.FindAsync(id);
            if (existingSchedule == null)
                return null;

            existingSchedule.StudioId = schedule.StudioId;
            existingSchedule.MovieId = schedule.MovieId;
            existingSchedule.ShowDateTime = schedule.ShowDateTime;
            existingSchedule.TicketPrice = schedule.TicketPrice;

            await _context.SaveChangesAsync();
            
            // Load related entities for return
            await _context.Entry(existingSchedule)
                .Reference(s => s.Studio)
                .LoadAsync();
            await _context.Entry(existingSchedule)
                .Reference(s => s.Movie)
                .LoadAsync();
                
            return existingSchedule;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return false;

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Schedules.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> StudioExistsAsync(int studioId)
        {
            return await _context.Studios.AnyAsync(s => s.Id == studioId);
        }

        public async Task<bool> MovieExistsAsync(int movieId)
        {
            return await _context.Movies.AnyAsync(m => m.Id == movieId);
        }
    }
}