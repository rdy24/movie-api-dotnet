using MovieApp.Models;

namespace MovieApp.Interfaces
{
    public interface IMovieRepository
    {
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<Movie?> GetByIdAsync(int id);
        Task<Movie> CreateAsync(Movie movie);
        Task<Movie?> UpdateAsync(int id, Movie movie);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}