using Microsoft.AspNetCore.Mvc;
using MovieApp.DTOs;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepository _movieRepository;

        public MoviesController(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies()
        {
            var movies = await _movieRepository.GetAllAsync();
            var movieDtos = movies.Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Genre = m.Genre,
                Duration = m.Duration,
                Description = m.Description
            });

            return Ok(movieDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDto>> GetMovie(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid movie ID");
            }

            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
            {
                return NotFound($"Movie with ID {id} not found");
            }

            var movieDto = new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                Duration = movie.Duration,
                Description = movie.Description
            };

            return Ok(movieDto);
        }

        [HttpPost]
        public async Task<ActionResult<MovieDto>> CreateMovie(CreateMovieDto createMovieDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var movie = new Movie
            {
                Title = createMovieDto.Title,
                Genre = createMovieDto.Genre,
                Duration = createMovieDto.Duration,
                Description = createMovieDto.Description
            };

            var createdMovie = await _movieRepository.CreateAsync(movie);

            var movieDto = new MovieDto
            {
                Id = createdMovie.Id,
                Title = createdMovie.Title,
                Genre = createdMovie.Genre,
                Duration = createdMovie.Duration,
                Description = createdMovie.Description
            };

            return CreatedAtAction(nameof(GetMovie), new { id = movieDto.Id }, movieDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MovieDto>> UpdateMovie(int id, UpdateMovieDto updateMovieDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0)
            {
                return BadRequest("Invalid movie ID");
            }

            var movie = new Movie
            {
                Title = updateMovieDto.Title,
                Genre = updateMovieDto.Genre,
                Duration = updateMovieDto.Duration,
                Description = updateMovieDto.Description
            };

            var updatedMovie = await _movieRepository.UpdateAsync(id, movie);
            if (updatedMovie == null)
            {
                return NotFound($"Movie with ID {id} not found");
            }

            var movieDto = new MovieDto
            {
                Id = updatedMovie.Id,
                Title = updatedMovie.Title,
                Genre = updatedMovie.Genre,
                Duration = updatedMovie.Duration,
                Description = updatedMovie.Description
            };

            return Ok(movieDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMovie(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid movie ID");
            }

            var result = await _movieRepository.DeleteAsync(id);
            if (!result)
            {
                return NotFound($"Movie with ID {id} not found");
            }

            return NoContent();
        }
    }
}