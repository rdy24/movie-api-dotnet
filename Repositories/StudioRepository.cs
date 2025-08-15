using Microsoft.EntityFrameworkCore;
using MovieApp.Data;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Repositories
{
    public class StudioRepository : IStudioRepository
    {
        private readonly CinemaDbContext _context;

        public StudioRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Studio>> GetAllAsync()
        {
            return await _context.Studios.ToListAsync();
        }

        public async Task<Studio?> GetByIdAsync(int id)
        {
            return await _context.Studios.FindAsync(id);
        }

        public async Task<Studio> CreateAsync(Studio studio)
        {
            _context.Studios.Add(studio);
            await _context.SaveChangesAsync();
            return studio;
        }

        public async Task<Studio?> UpdateAsync(int id, Studio studio)
        {
            var existingStudio = await _context.Studios.FindAsync(id);
            if (existingStudio == null)
                return null;

            existingStudio.Name = studio.Name;
            existingStudio.Capacity = studio.Capacity;
            existingStudio.Facilities = studio.Facilities;

            await _context.SaveChangesAsync();
            return existingStudio;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var studio = await _context.Studios.FindAsync(id);
            if (studio == null)
                return false;

            _context.Studios.Remove(studio);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Studios.AnyAsync(s => s.Id == id);
        }
    }
}