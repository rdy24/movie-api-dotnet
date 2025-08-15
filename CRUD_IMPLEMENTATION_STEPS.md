# How to Create CRUD API with Repository Pattern - Step by Step

## Step 1: Create DTOs (Data Transfer Objects)

### 1.1 Create DTOs folder
```bash
mkdir DTOs
```

### 1.2 Create StudioDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

namespace MovieApp.DTOs
{
    public class StudioDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? Facilities { get; set; }
    }

    public class CreateStudioDto
    {
        [Required(ErrorMessage = "Studio name is required")]
        [StringLength(100, ErrorMessage = "Studio name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        public int Capacity { get; set; }

        [StringLength(500, ErrorMessage = "Facilities description cannot exceed 500 characters")]
        public string? Facilities { get; set; }
    }

    public class UpdateStudioDto
    {
        [Required(ErrorMessage = "Studio name is required")]
        [StringLength(100, ErrorMessage = "Studio name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        public int Capacity { get; set; }

        [StringLength(500, ErrorMessage = "Facilities description cannot exceed 500 characters")]
        public string? Facilities { get; set; }
    }
}
```

## Step 2: Create Repository Interface

### 2.1 Create Interfaces folder
```bash
mkdir Interfaces
```

### 2.2 Create IStudioRepository.cs
```csharp
using MovieApp.Models;

namespace MovieApp.Interfaces
{
    public interface IStudioRepository
    {
        Task<IEnumerable<Studio>> GetAllAsync();
        Task<Studio?> GetByIdAsync(int id);
        Task<Studio> CreateAsync(Studio studio);
        Task<Studio?> UpdateAsync(int id, Studio studio);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
```

## Step 3: Create Repository Implementation

### 3.1 Create Repositories folder
```bash
mkdir Repositories
```

### 3.2 Create StudioRepository.cs
```csharp
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
```

## Step 4: Create Controller

### 4.1 Create StudiosController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using MovieApp.DTOs;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudiosController : ControllerBase
    {
        private readonly IStudioRepository _studioRepository;

        public StudiosController(IStudioRepository studioRepository)
        {
            _studioRepository = studioRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudioDto>>> GetStudios()
        {
            var studios = await _studioRepository.GetAllAsync();
            var studioDtos = studios.Select(s => new StudioDto
            {
                Id = s.Id,
                Name = s.Name,
                Capacity = s.Capacity,
                Facilities = s.Facilities
            });

            return Ok(studioDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudioDto>> GetStudio(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid studio ID");
            }

            var studio = await _studioRepository.GetByIdAsync(id);
            if (studio == null)
            {
                return NotFound($"Studio with ID {id} not found");
            }

            var studioDto = new StudioDto
            {
                Id = studio.Id,
                Name = studio.Name,
                Capacity = studio.Capacity,
                Facilities = studio.Facilities
            };

            return Ok(studioDto);
        }

        [HttpPost]
        public async Task<ActionResult<StudioDto>> CreateStudio(CreateStudioDto createStudioDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var studio = new Studio
            {
                Name = createStudioDto.Name,
                Capacity = createStudioDto.Capacity,
                Facilities = createStudioDto.Facilities
            };

            var createdStudio = await _studioRepository.CreateAsync(studio);

            var studioDto = new StudioDto
            {
                Id = createdStudio.Id,
                Name = createdStudio.Name,
                Capacity = createdStudio.Capacity,
                Facilities = createdStudio.Facilities
            };

            return CreatedAtAction(nameof(GetStudio), new { id = studioDto.Id }, studioDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StudioDto>> UpdateStudio(int id, UpdateStudioDto updateStudioDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0)
            {
                return BadRequest("Invalid studio ID");
            }

            var studio = new Studio
            {
                Name = updateStudioDto.Name,
                Capacity = updateStudioDto.Capacity,
                Facilities = updateStudioDto.Facilities
            };

            var updatedStudio = await _studioRepository.UpdateAsync(id, studio);
            if (updatedStudio == null)
            {
                return NotFound($"Studio with ID {id} not found");
            }

            var studioDto = new StudioDto
            {
                Id = updatedStudio.Id,
                Name = updatedStudio.Name,
                Capacity = updatedStudio.Capacity,
                Facilities = updatedStudio.Facilities
            };

            return Ok(studioDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStudio(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid studio ID");
            }

            var result = await _studioRepository.DeleteAsync(id);
            if (!result)
            {
                return NotFound($"Studio with ID {id} not found");
            }

            return NoContent();
        }
    }
}
```

## Step 5: Register Repository in Dependency Injection

### 5.1 Update Program.cs
Add these using statements:
```csharp
using MovieApp.Interfaces;
using MovieApp.Repositories;
```

Add repository registration:
```csharp
// Register repositories
builder.Services.AddScoped<IStudioRepository, StudioRepository>();
```

Complete Program.cs should look like:
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

## Step 6: Test the API

### 6.1 Build and Run
```bash
dotnet build
dotnet run
```

### 6.2 Test Endpoints
Navigate to: `https://localhost:5090/swagger`

Or test with cURL:
```bash
# Get all studios
curl -X GET "http://localhost:5090/api/studios"

# Create studio
curl -X POST "http://localhost:5090/api/studios" \
  -H "Content-Type: application/json" \
  -d '{"name": "Studio Test", "capacity": 100, "facilities": "Test facilities"}'
```

## Project Structure After Implementation

```
MovieApp/
├── Controllers/
│   └── StudiosController.cs
├── DTOs/
│   └── StudioDto.cs
├── Interfaces/
│   └── IStudioRepository.cs
├── Repositories/
│   └── StudioRepository.cs
├── Models/
│   └── Studio.cs
├── Data/
│   └── CinemaDbContext.cs
└── Program.cs
```

## Key Benefits of This Pattern

1. **Separation of Concerns**: Controllers handle HTTP, repositories handle data access
2. **Testability**: Easy to mock repositories for unit testing
3. **Maintainability**: Clear structure and responsibilities
4. **Validation**: Input validation with data annotations
5. **Error Handling**: Proper HTTP status codes and error messages
6. **Clean Architecture**: Following SOLID principles