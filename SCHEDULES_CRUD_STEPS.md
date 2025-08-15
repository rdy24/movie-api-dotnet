# Schedule CRUD Implementation Steps

## Step 1: Create Schedule DTOs

### File: `DTOs/ScheduleDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace MovieApp.DTOs
{
    public class ScheduleDto
    {
        public int Id { get; set; }
        public int StudioId { get; set; }
        public string StudioName { get; set; } = string.Empty;
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime ShowDateTime { get; set; }
        public decimal TicketPrice { get; set; }
    }

    public class CreateScheduleDto
    {
        [Required(ErrorMessage = "Studio ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Studio ID must be a positive number")]
        public int StudioId { get; set; }

        [Required(ErrorMessage = "Movie ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Movie ID must be a positive number")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Show date and time is required")]
        public DateTime ShowDateTime { get; set; }

        [Required(ErrorMessage = "Ticket price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Ticket price must be between 0.01 and 1,000,000")]
        public decimal TicketPrice { get; set; }
    }

    public class UpdateScheduleDto
    {
        [Required(ErrorMessage = "Studio ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Studio ID must be a positive number")]
        public int StudioId { get; set; }

        [Required(ErrorMessage = "Movie ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Movie ID must be a positive number")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Show date and time is required")]
        public DateTime ShowDateTime { get; set; }

        [Required(ErrorMessage = "Ticket price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Ticket price must be between 0.01 and 1,000,000")]
        public decimal TicketPrice { get; set; }
    }
}
```

**Validation Rules:**
- StudioId: Required, positive integer
- MovieId: Required, positive integer
- ShowDateTime: Required, valid DateTime
- TicketPrice: Required, between 0.01 and 1,000,000

**Key Features:**
- `ScheduleDto` includes related entity names (StudioName, MovieTitle) for better API response
- Foreign key validation with range checks
- Decimal validation for ticket price

## Step 2: Create Schedule Repository Interface

### File: `Interfaces/IScheduleRepository.cs`

```csharp
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
```

**Additional Methods:**
- `StudioExistsAsync(int studioId)` - Validate studio exists before creating/updating schedule
- `MovieExistsAsync(int movieId)` - Validate movie exists before creating/updating schedule

## Step 3: Create Schedule Repository Implementation

### File: `Repositories/ScheduleRepository.cs`

```csharp
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
```

**Key Features:**
- Uses `Include()` to load related Studio and Movie entities
- Uses `LoadAsync()` to explicitly load navigation properties after create/update
- Additional validation methods for foreign key integrity
- Proper async/await pattern throughout

## Step 4: Create Schedules Controller

### File: `Controllers/SchedulesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using MovieApp.DTOs;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleRepository;

        public SchedulesController(IScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedules()
        {
            var schedules = await _scheduleRepository.GetAllAsync();
            var scheduleDtos = schedules.Select(s => new ScheduleDto
            {
                Id = s.Id,
                StudioId = s.StudioId,
                StudioName = s.Studio.Name,
                MovieId = s.MovieId,
                MovieTitle = s.Movie.Title,
                ShowDateTime = s.ShowDateTime,
                TicketPrice = s.TicketPrice
            });

            return Ok(scheduleDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleDto>> GetSchedule(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid schedule ID");
            }

            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
            {
                return NotFound($"Schedule with ID {id} not found");
            }

            var scheduleDto = new ScheduleDto
            {
                Id = schedule.Id,
                StudioId = schedule.StudioId,
                StudioName = schedule.Studio.Name,
                MovieId = schedule.MovieId,
                MovieTitle = schedule.Movie.Title,
                ShowDateTime = schedule.ShowDateTime,
                TicketPrice = schedule.TicketPrice
            };

            return Ok(scheduleDto);
        }

        [HttpPost]
        public async Task<ActionResult<ScheduleDto>> CreateSchedule(CreateScheduleDto createScheduleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate if Studio exists
            if (!await _scheduleRepository.StudioExistsAsync(createScheduleDto.StudioId))
            {
                return BadRequest($"Studio with ID {createScheduleDto.StudioId} does not exist");
            }

            // Validate if Movie exists
            if (!await _scheduleRepository.MovieExistsAsync(createScheduleDto.MovieId))
            {
                return BadRequest($"Movie with ID {createScheduleDto.MovieId} does not exist");
            }

            // Validate show date is in the future
            if (createScheduleDto.ShowDateTime <= DateTime.Now)
            {
                return BadRequest("Show date and time must be in the future");
            }

            var schedule = new Schedule
            {
                StudioId = createScheduleDto.StudioId,
                MovieId = createScheduleDto.MovieId,
                ShowDateTime = createScheduleDto.ShowDateTime,
                TicketPrice = createScheduleDto.TicketPrice
            };

            var createdSchedule = await _scheduleRepository.CreateAsync(schedule);

            var scheduleDto = new ScheduleDto
            {
                Id = createdSchedule.Id,
                StudioId = createdSchedule.StudioId,
                StudioName = createdSchedule.Studio.Name,
                MovieId = createdSchedule.MovieId,
                MovieTitle = createdSchedule.Movie.Title,
                ShowDateTime = createdSchedule.ShowDateTime,
                TicketPrice = createdSchedule.TicketPrice
            };

            return CreatedAtAction(nameof(GetSchedule), new { id = scheduleDto.Id }, scheduleDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ScheduleDto>> UpdateSchedule(int id, UpdateScheduleDto updateScheduleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0)
            {
                return BadRequest("Invalid schedule ID");
            }

            // Validate if Studio exists
            if (!await _scheduleRepository.StudioExistsAsync(updateScheduleDto.StudioId))
            {
                return BadRequest($"Studio with ID {updateScheduleDto.StudioId} does not exist");
            }

            // Validate if Movie exists
            if (!await _scheduleRepository.MovieExistsAsync(updateScheduleDto.MovieId))
            {
                return BadRequest($"Movie with ID {updateScheduleDto.MovieId} does not exist");
            }

            // Validate show date is in the future
            if (updateScheduleDto.ShowDateTime <= DateTime.Now)
            {
                return BadRequest("Show date and time must be in the future");
            }

            var schedule = new Schedule
            {
                StudioId = updateScheduleDto.StudioId,
                MovieId = updateScheduleDto.MovieId,
                ShowDateTime = updateScheduleDto.ShowDateTime,
                TicketPrice = updateScheduleDto.TicketPrice
            };

            var updatedSchedule = await _scheduleRepository.UpdateAsync(id, schedule);
            if (updatedSchedule == null)
            {
                return NotFound($"Schedule with ID {id} not found");
            }

            var scheduleDto = new ScheduleDto
            {
                Id = updatedSchedule.Id,
                StudioId = updatedSchedule.StudioId,
                StudioName = updatedSchedule.Studio.Name,
                MovieId = updatedSchedule.MovieId,
                MovieTitle = updatedSchedule.Movie.Title,
                ShowDateTime = updatedSchedule.ShowDateTime,
                TicketPrice = updatedSchedule.TicketPrice
            };

            return Ok(scheduleDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSchedule(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid schedule ID");
            }

            var result = await _scheduleRepository.DeleteAsync(id);
            if (!result)
            {
                return NotFound($"Schedule with ID {id} not found");
            }

            return NoContent();
        }
    }
}
```

**Advanced Validation Features:**
- **Foreign Key Validation**: Checks if Studio and Movie exist before creating/updating
- **Business Logic Validation**: Ensures ShowDateTime is in the future
- **Rich Error Messages**: Specific error messages for different validation failures
- **Related Data**: Returns Studio name and Movie title in responses

## Step 5: Register Repository in Dependency Injection

### Update `Program.cs`

Add ScheduleRepository registration:

```csharp
// Register repositories
builder.Services.AddScoped<IStudioRepository, StudioRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
```

Complete registration section:

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
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

## Step 6: Test the Implementation

### Build and Run
```bash
dotnet build
dotnet run
```

### Test Endpoints
Navigate to: `https://localhost:5090/swagger`

### Sample JSON for Testing

#### Create Schedule:
```json
{
  "studioId": 1,
  "movieId": 1,
  "showDateTime": "2024-12-25T19:00:00",
  "ticketPrice": 75000
}
```

#### Update Schedule:
```json
{
  "studioId": 2,
  "movieId": 2,
  "showDateTime": "2024-12-26T21:30:00",
  "ticketPrice": 85000
}
```

### Project Structure After Implementation

```
MovieApp/
├── Controllers/
│   ├── MoviesController.cs
│   ├── SchedulesController.cs
│   └── StudiosController.cs
├── DTOs/
│   ├── MovieDto.cs
│   ├── ScheduleDto.cs
│   └── StudioDto.cs
├── Interfaces/
│   ├── IMovieRepository.cs
│   ├── IScheduleRepository.cs
│   └── IStudioRepository.cs
├── Repositories/
│   ├── MovieRepository.cs
│   ├── ScheduleRepository.cs
│   └── StudioRepository.cs
├── Models/
│   ├── Movie.cs
│   ├── Schedule.cs
│   └── Studio.cs
├── Data/
│   └── CinemaDbContext.cs
└── Program.cs
```

## Summary

✅ **Created Schedule DTOs** with comprehensive validation rules
✅ **Created IScheduleRepository interface** with foreign key validation methods
✅ **Implemented ScheduleRepository** with Entity Framework Include operations
✅ **Created SchedulesController** with advanced business logic validation
✅ **Registered repository** in dependency injection container

**API Endpoints Available:**
- `GET /api/schedules` - Get all schedules with Studio and Movie names
- `GET /api/schedules/{id}` - Get schedule by ID with related data
- `POST /api/schedules` - Create new schedule with validation
- `PUT /api/schedules/{id}` - Update schedule with validation
- `DELETE /api/schedules/{id}` - Delete schedule

**Advanced Features:**
- **Foreign Key Validation**: Validates Studio and Movie existence
- **Business Rules**: Ensures show times are in the future
- **Rich Responses**: Includes related entity names in DTOs
- **Comprehensive Error Handling**: Specific error messages for different scenarios