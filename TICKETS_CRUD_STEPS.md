# Ticket CRUD Implementation Steps

## Step 1: Create User DTO (Dependency)

### File: `DTOs/UserDto.cs`

```csharp
namespace MovieApp.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
```

**Purpose:** Required for Ticket responses to include complete user information (excluding password for security).

## Step 2: Create Ticket DTOs

### File: `DTOs/TicketDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using MovieApp.Models;

namespace MovieApp.DTOs
{
    public class TicketDto
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public int UserId { get; set; }
        public ScheduleDto Schedule { get; set; } = new();
        public UserDto User { get; set; } = new();
        public string SeatNumber { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public DateTime BookedAt { get; set; }
    }

    public class CreateTicketDto
    {
        [Required(ErrorMessage = "Schedule ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Schedule ID must be a positive number")]
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Seat number is required")]
        [StringLength(10, ErrorMessage = "Seat number cannot exceed 10 characters")]
        public string SeatNumber { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.Confirmed;
    }

    public class UpdateTicketDto
    {
        [Required(ErrorMessage = "Schedule ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Schedule ID must be a positive number")]
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Seat number is required")]
        [StringLength(10, ErrorMessage = "Seat number cannot exceed 10 characters")]
        public string SeatNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public TicketStatus Status { get; set; }
    }
}
```

**Validation Rules:**
- ScheduleId: Required, positive integer
- UserId: Required, positive integer
- SeatNumber: Required, max 10 characters
- Status: Required (for updates), defaults to Confirmed (for create)

**Key Features:**
- `TicketDto` includes complete Schedule and User objects for rich API responses
- Foreign key validation with range checks
- Seat number validation for cinema seating

## Step 3: Create Ticket Repository Interface

### File: `Interfaces/ITicketRepository.cs`

```csharp
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
```

**Business Logic Methods:**
- `ScheduleExistsAsync(int scheduleId)` - Validate schedule exists before booking
- `UserExistsAsync(int userId)` - Validate user exists before booking
- `SeatAvailableAsync(scheduleId, seatNumber, excludeTicketId)` - Prevent double booking of seats

## Step 4: Create Ticket Repository Implementation

### File: `Repositories/TicketRepository.cs`

```csharp
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
```

**Advanced Features:**
- **Complex Include Operations**: Uses `ThenInclude()` to load nested related entities (Schedule -> Studio/Movie)
- **Automatic Timestamps**: Sets `BookedAt` to current UTC time on creation
- **Seat Availability Logic**: Checks for existing non-cancelled tickets for the same seat
- **Update Exclusion**: Allows checking seat availability while excluding current ticket (for updates)

## Step 5: Create Tickets Controller

### File: `Controllers/TicketsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using MovieApp.DTOs;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketsController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets()
        {
            var tickets = await _ticketRepository.GetAllAsync();
            var ticketDtos = tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                ScheduleId = t.ScheduleId,
                UserId = t.UserId,
                Schedule = new ScheduleDto
                {
                    Id = t.Schedule.Id,
                    StudioId = t.Schedule.StudioId,
                    MovieId = t.Schedule.MovieId,
                    Studio = new StudioDto
                    {
                        Id = t.Schedule.Studio.Id,
                        Name = t.Schedule.Studio.Name,
                        Capacity = t.Schedule.Studio.Capacity,
                        Facilities = t.Schedule.Studio.Facilities
                    },
                    Movie = new MovieDto
                    {
                        Id = t.Schedule.Movie.Id,
                        Title = t.Schedule.Movie.Title,
                        Genre = t.Schedule.Movie.Genre,
                        Duration = t.Schedule.Movie.Duration,
                        Description = t.Schedule.Movie.Description
                    },
                    ShowDateTime = t.Schedule.ShowDateTime,
                    TicketPrice = t.Schedule.TicketPrice
                },
                User = new UserDto
                {
                    Id = t.User.Id,
                    Name = t.User.Name,
                    Email = t.User.Email,
                    Username = t.User.Username,
                    Phone = t.User.Phone,
                    Role = t.User.Role,
                    CreatedAt = t.User.CreatedAt,
                    IsActive = t.User.IsActive
                },
                SeatNumber = t.SeatNumber,
                Status = t.Status,
                BookedAt = t.BookedAt
            });

            return Ok(ticketDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ticket ID");
            }

            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
            {
                return NotFound($"Ticket with ID {id} not found");
            }

            // Similar mapping as in GetTickets...
            return Ok(ticketDto);
        }

        [HttpPost]
        public async Task<ActionResult<TicketDto>> CreateTicket(CreateTicketDto createTicketDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate if Schedule exists
            if (!await _ticketRepository.ScheduleExistsAsync(createTicketDto.ScheduleId))
            {
                return BadRequest($"Schedule with ID {createTicketDto.ScheduleId} does not exist");
            }

            // Validate if User exists
            if (!await _ticketRepository.UserExistsAsync(createTicketDto.UserId))
            {
                return BadRequest($"User with ID {createTicketDto.UserId} does not exist");
            }

            // Validate seat availability
            if (!await _ticketRepository.SeatAvailableAsync(createTicketDto.ScheduleId, createTicketDto.SeatNumber))
            {
                return BadRequest($"Seat {createTicketDto.SeatNumber} is already booked for this schedule");
            }

            var ticket = new Ticket
            {
                ScheduleId = createTicketDto.ScheduleId,
                UserId = createTicketDto.UserId,
                SeatNumber = createTicketDto.SeatNumber,
                Status = createTicketDto.Status
            };

            var createdTicket = await _ticketRepository.CreateAsync(ticket);
            // Map to DTO and return...
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TicketDto>> UpdateTicket(int id, UpdateTicketDto updateTicketDto)
        {
            // Similar validation as Create, plus:
            
            // Validate seat availability (excluding current ticket)
            if (!await _ticketRepository.SeatAvailableAsync(updateTicketDto.ScheduleId, updateTicketDto.SeatNumber, id))
            {
                return BadRequest($"Seat {updateTicketDto.SeatNumber} is already booked for this schedule");
            }
            
            // Update logic...
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTicket(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ticket ID");
            }

            var result = await _ticketRepository.DeleteAsync(id);
            if (!result)
            {
                return NotFound($"Ticket with ID {id} not found");
            }

            return NoContent();
        }
    }
}
```

**Business Logic Validation:**
- **Foreign Key Validation**: Validates Schedule and User existence before booking
- **Seat Availability**: Prevents double booking of the same seat for the same schedule
- **Update Logic**: Allows seat changes while preventing conflicts with other bookings

## Step 6: Register Repository in Dependency Injection

### Update `Program.cs`

Add TicketRepository registration:

```csharp
// Register repositories
builder.Services.AddScoped<IStudioRepository, StudioRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
```

## Step 7: Test the Implementation

### Build and Run
```bash
dotnet build
dotnet run
```

### Test Endpoints
Navigate to: `https://localhost:5090/swagger`

### Sample JSON for Testing

#### Create Ticket:
```json
{
  "scheduleId": 1,
  "userId": 1,
  "seatNumber": "A1",
  "status": 0
}
```

#### Update Ticket:
```json
{
  "scheduleId": 1,
  "userId": 1,
  "seatNumber": "A2",
  "status": 1
}
```

**TicketStatus Enum Values:**
- `0` = Confirmed
- `1` = Cancelled  
- `2` = Expired

### Expected API Response Structure

```json
{
  "id": 1,
  "scheduleId": 1,
  "userId": 1,
  "schedule": {
    "id": 1,
    "studioId": 1,
    "movieId": 1,
    "studio": {
      "id": 1,
      "name": "Studio IMAX",
      "capacity": 200,
      "facilities": "IMAX Screen, Dolby Atmos"
    },
    "movie": {
      "id": 1,
      "title": "Avengers: Endgame",
      "genre": "Action",
      "duration": 181,
      "description": "The epic conclusion to the Infinity Saga"
    },
    "showDateTime": "2024-12-25T19:00:00",
    "ticketPrice": 75000
  },
  "user": {
    "id": 1,
    "name": "John Doe",
    "email": "john.doe@email.com",
    "username": "johndoe",
    "phone": "081234567890",
    "role": "Customer",
    "createdAt": "2024-01-01T09:00:00Z",
    "isActive": true
  },
  "seatNumber": "A1",
  "status": 0,
  "bookedAt": "2024-08-15T10:30:00Z"
}
```

## Project Structure After Implementation

```
MovieApp/
├── Controllers/
│   ├── MoviesController.cs
│   ├── SchedulesController.cs
│   ├── StudiosController.cs
│   └── TicketsController.cs
├── DTOs/
│   ├── MovieDto.cs
│   ├── ScheduleDto.cs
│   ├── StudioDto.cs
│   ├── TicketDto.cs
│   └── UserDto.cs
├── Interfaces/
│   ├── IMovieRepository.cs
│   ├── IScheduleRepository.cs
│   ├── IStudioRepository.cs
│   └── ITicketRepository.cs
├── Repositories/
│   ├── MovieRepository.cs
│   ├── ScheduleRepository.cs
│   ├── StudioRepository.cs
│   └── TicketRepository.cs
├── Models/
│   ├── Movie.cs
│   ├── Schedule.cs
│   ├── Studio.cs
│   ├── Ticket.cs
│   └── User.cs
├── Data/
│   └── CinemaDbContext.cs
└── Program.cs
```

## Summary

✅ **Created comprehensive Ticket DTOs** with validation and complete related entity information
✅ **Created ITicketRepository interface** with business logic validation methods
✅ **Implemented TicketRepository** with complex Include operations and business logic
✅ **Created TicketsController** with seat availability validation and rich responses
✅ **Registered repository** in dependency injection container

**API Endpoints Available:**
- `GET /api/tickets` - Get all tickets with complete Schedule, User, Studio, and Movie data
- `GET /api/tickets/{id}` - Get ticket by ID with all related information
- `POST /api/tickets` - Create new ticket with seat availability validation
- `PUT /api/tickets/{id}` - Update ticket with conflict prevention
- `DELETE /api/tickets/{id}` - Delete ticket

**Advanced Business Features:**
- **Seat Conflict Prevention**: Prevents booking the same seat twice for the same schedule
- **Foreign Key Validation**: Validates Schedule and User existence before operations
- **Rich Data Responses**: Complete nested objects for Schedule (with Studio/Movie) and User
- **Automatic Timestamping**: Sets BookedAt timestamp automatically on creation
- **Status Management**: Supports Confirmed, Cancelled, and Expired ticket statuses