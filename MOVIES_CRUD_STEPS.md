# Movie CRUD Implementation Steps

## Step 1: Create Movie DTOs

### File: `DTOs/MovieDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace MovieApp.DTOs
{
    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public int Duration { get; set; }
        public string? Description { get; set; }
    }

    public class CreateMovieDto
    {
        [Required(ErrorMessage = "Movie title is required")]
        [StringLength(200, ErrorMessage = "Movie title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Genre cannot exceed 50 characters")]
        public string? Genre { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes")]
        public int Duration { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }

    public class UpdateMovieDto
    {
        [Required(ErrorMessage = "Movie title is required")]
        [StringLength(200, ErrorMessage = "Movie title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Genre cannot exceed 50 characters")]
        public string? Genre { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes")]
        public int Duration { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }
}
```

**Validation Rules:**
- Title: Required, max 200 characters
- Genre: Optional, max 50 characters  
- Duration: Required, 1-600 minutes
- Description: Optional, max 1000 characters

## Step 2: Create Movie Repository Interface

### File: `Interfaces/IMovieRepository.cs`

```csharp
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
```

**Methods Explained:**
- `GetAllAsync()` - Retrieve all movies
- `GetByIdAsync(id)` - Get specific movie by ID
- `CreateAsync(movie)` - Create new movie
- `UpdateAsync(id, movie)` - Update existing movie
- `DeleteAsync(id)` - Delete movie by ID
- `ExistsAsync(id)` - Check if movie exists

## Step 3: Create Movie Repository Implementation

### File: `Repositories/MovieRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using MovieApp.Data;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly CinemaDbContext _context;

        public MovieRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            return await _context.Movies.ToListAsync();
        }

        public async Task<Movie?> GetByIdAsync(int id)
        {
            return await _context.Movies.FindAsync(id);
        }

        public async Task<Movie> CreateAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task<Movie?> UpdateAsync(int id, Movie movie)
        {
            var existingMovie = await _context.Movies.FindAsync(id);
            if (existingMovie == null)
                return null;

            existingMovie.Title = movie.Title;
            existingMovie.Genre = movie.Genre;
            existingMovie.Duration = movie.Duration;
            existingMovie.Description = movie.Description;

            await _context.SaveChangesAsync();
            return existingMovie;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return false;

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Movies.AnyAsync(m => m.Id == id);
        }
    }
}
```

**Key Points:**
- Constructor injection of DbContext
- Async/await pattern for all operations
- Proper null checking in Update and Delete
- Returns appropriate types for each operation

## Step 4: Create Movies Controller

### File: `Controllers/MoviesController.cs`

```csharp
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
```

**HTTP Methods & Status Codes:**
- GET `/api/movies` → 200 OK
- GET `/api/movies/{id}` → 200 OK / 404 Not Found / 400 Bad Request
- POST `/api/movies` → 201 Created / 400 Bad Request
- PUT `/api/movies/{id}` → 200 OK / 404 Not Found / 400 Bad Request
- DELETE `/api/movies/{id}` → 204 No Content / 404 Not Found / 400 Bad Request

## Step 5: Register Repository in Dependency Injection

### Update `Program.cs`

Add MovieRepository registration:

```csharp
// Register repositories
builder.Services.AddScoped<IStudioRepository, StudioRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
```

Complete Program.cs:

```csharp
using Microsoft.EntityFrameworkCore;
using MovieApp.Data;
using MovieApp.Interfaces;
using MovieApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IStudioRepository, StudioRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
    context.Database.Migrate();
    SeedData.Seed(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Step 6: Test the Implementation

### Build and Run
```bash
dotnet build
dotnet run
```

### Test Endpoints
Navigate to: `https://localhost:5090/swagger`

### Project Structure
```
MovieApp/
├── Controllers/
│   ├── MoviesController.cs
│   └── StudiosController.cs
├── DTOs/
│   ├── MovieDto.cs
│   └── StudioDto.cs
├── Interfaces/
│   ├── IMovieRepository.cs
│   └── IStudioRepository.cs
├── Repositories/
│   ├── MovieRepository.cs
│   └── StudioRepository.cs
├── Models/
│   ├── Movie.cs
│   └── Studio.cs
├── Data/
│   └── CinemaDbContext.cs
└── Program.cs
```

## Summary

✅ **Created Movie DTOs** with validation rules
✅ **Created IMovieRepository interface** with standard CRUD methods
✅ **Implemented MovieRepository** with Entity Framework operations
✅ **Created MoviesController** with proper error handling
✅ **Registered repository** in dependency injection container

**API Endpoints Available:**
- `GET /api/movies` - Get all movies
- `GET /api/movies/{id}` - Get movie by ID
- `POST /api/movies` - Create new movie
- `PUT /api/movies/{id}` - Update movie
- `DELETE /api/movies/{id}` - Delete movie