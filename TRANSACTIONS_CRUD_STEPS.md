# Transaction CRUD Implementation Steps

## Step 1: Create Transaction DTOs

### File: `DTOs/TransactionDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using MovieApp.Models;

namespace MovieApp.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public TicketDto Ticket { get; set; } = new();
        public UserDto User { get; set; } = new();
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? PaymentReference { get; set; }
    }

    public class CreateTransactionDto
    {
        [Required(ErrorMessage = "Ticket ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Ticket ID must be a positive number")]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 10000000, ErrorMessage = "Amount must be between 0.01 and 10,000,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [StringLength(100, ErrorMessage = "Payment reference cannot exceed 100 characters")]
        public string? PaymentReference { get; set; }
    }

    public class UpdateTransactionDto
    {
        [Required(ErrorMessage = "Ticket ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Ticket ID must be a positive number")]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 10000000, ErrorMessage = "Amount must be between 0.01 and 10,000,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Payment status is required")]
        public PaymentStatus PaymentStatus { get; set; }

        [StringLength(100, ErrorMessage = "Payment reference cannot exceed 100 characters")]
        public string? PaymentReference { get; set; }
    }
}
```

**Validation Rules:**
- TicketId: Required, positive integer
- UserId: Required, positive integer
- Amount: Required, between 0.01 and 10,000,000
- PaymentMethod: Required enum (CreditCard, EWallet, BankTransfer)
- PaymentStatus: Defaults to Pending for create, required for update
- PaymentReference: Optional, max 100 characters

**Key Features:**
- `TransactionDto` includes complete Ticket object (with full Schedule, Studio, Movie data) and User
- Payment amount validation for realistic transaction amounts
- Payment method and status enums for structured data

## Step 2: Create Transaction Repository Interface

### File: `Interfaces/ITransactionRepository.cs`

```csharp
using MovieApp.Models;

namespace MovieApp.Interfaces
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction> CreateAsync(Transaction transaction);
        Task<Transaction?> UpdateAsync(int id, Transaction transaction);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TicketExistsAsync(int ticketId);
        Task<bool> UserExistsAsync(int userId);
        Task<bool> TicketAlreadyPaidAsync(int ticketId, int? excludeTransactionId = null);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Transaction>> GetByTicketIdAsync(int ticketId);
    }
}
```

**Business Logic Methods:**
- `TicketExistsAsync(int ticketId)` - Validate ticket exists before payment
- `UserExistsAsync(int userId)` - Validate user exists before payment
- `TicketAlreadyPaidAsync(ticketId, excludeTransactionId)` - Prevent duplicate successful payments
- `GetByUserIdAsync(int userId)` - Get user's transaction history
- `GetByTicketIdAsync(int ticketId)` - Get all transactions for a specific ticket

## Step 3: Create Transaction Repository Implementation

### File: `Repositories/TransactionRepository.cs`

```csharp
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
            
            // Load all related entities for rich response
            // (Complex loading code...)
            
            return transaction;
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

        // Similar implementation for GetByTicketIdAsync...
    }
}
```

**Advanced Features:**
- **Deep Include Operations**: Multiple levels of `ThenInclude()` to load complete transaction context
- **Automatic Timestamps**: Sets `TransactionDate` to current UTC time
- **Payment Validation**: Prevents duplicate successful payments for the same ticket
- **Ordered Results**: Transactions ordered by date (newest first) for better UX
- **Flexible Queries**: Support for user-specific and ticket-specific transaction history

## Step 4: Create Transactions Controller

### File: `Controllers/TransactionsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using MovieApp.DTOs;
using MovieApp.Models;
using MovieApp.Interfaces;

namespace MovieApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionsController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions()
        {
            var transactions = await _transactionRepository.GetAllAsync();
            var transactionDtos = transactions.Select(t => MapToDto(t));
            return Ok(transactionDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDto>> GetTransaction(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid transaction ID");
            }

            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound($"Transaction with ID {id} not found");
            }

            var transactionDto = MapToDto(transaction);
            return Ok(transactionDto);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByUser(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID");
            }

            if (!await _transactionRepository.UserExistsAsync(userId))
            {
                return NotFound($"User with ID {userId} not found");
            }

            var transactions = await _transactionRepository.GetByUserIdAsync(userId);
            var transactionDtos = transactions.Select(t => MapToDto(t));
            return Ok(transactionDtos);
        }

        [HttpGet("ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByTicket(int ticketId)
        {
            if (ticketId <= 0)
            {
                return BadRequest("Invalid ticket ID");
            }

            if (!await _transactionRepository.TicketExistsAsync(ticketId))
            {
                return NotFound($"Ticket with ID {ticketId} not found");
            }

            var transactions = await _transactionRepository.GetByTicketIdAsync(ticketId);
            var transactionDtos = transactions.Select(t => MapToDto(t));
            return Ok(transactionDtos);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> CreateTransaction(CreateTransactionDto createTransactionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate if Ticket exists
            if (!await _transactionRepository.TicketExistsAsync(createTransactionDto.TicketId))
            {
                return BadRequest($"Ticket with ID {createTransactionDto.TicketId} does not exist");
            }

            // Validate if User exists
            if (!await _transactionRepository.UserExistsAsync(createTransactionDto.UserId))
            {
                return BadRequest($"User with ID {createTransactionDto.UserId} does not exist");
            }

            // Check if ticket is already paid (for Success status)
            if (createTransactionDto.PaymentStatus == PaymentStatus.Success)
            {
                if (await _transactionRepository.TicketAlreadyPaidAsync(createTransactionDto.TicketId))
                {
                    return BadRequest($"Ticket with ID {createTransactionDto.TicketId} is already paid");
                }
            }

            var transaction = new Transaction
            {
                TicketId = createTransactionDto.TicketId,
                UserId = createTransactionDto.UserId,
                Amount = createTransactionDto.Amount,
                PaymentMethod = createTransactionDto.PaymentMethod,
                PaymentStatus = createTransactionDto.PaymentStatus,
                PaymentReference = createTransactionDto.PaymentReference
            };

            var createdTransaction = await _transactionRepository.CreateAsync(transaction);
            var transactionDto = MapToDto(createdTransaction);

            return CreatedAtAction(nameof(GetTransaction), new { id = transactionDto.Id }, transactionDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TransactionDto>> UpdateTransaction(int id, UpdateTransactionDto updateTransactionDto)
        {
            // Similar validation as Create, plus:
            
            // Check duplicate payment (excluding current transaction)
            if (updateTransactionDto.PaymentStatus == PaymentStatus.Success)
            {
                if (await _transactionRepository.TicketAlreadyPaidAsync(updateTransactionDto.TicketId, id))
                {
                    return BadRequest($"Ticket with ID {updateTransactionDto.TicketId} is already paid by another transaction");
                }
            }
            
            // Update logic...
        }

        private static TransactionDto MapToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                TicketId = transaction.TicketId,
                UserId = transaction.UserId,
                Ticket = new TicketDto
                {
                    // Complete ticket mapping with Schedule, Studio, Movie, User...
                },
                User = new UserDto
                {
                    // Complete user mapping...
                },
                Amount = transaction.Amount,
                PaymentMethod = transaction.PaymentMethod,
                PaymentStatus = transaction.PaymentStatus,
                TransactionDate = transaction.TransactionDate,
                PaymentReference = transaction.PaymentReference
            };
        }
    }
}
```

**Advanced Endpoints:**
- **GET /api/transactions/user/{userId}** - Get transaction history for specific user
- **GET /api/transactions/ticket/{ticketId}** - Get all payment attempts for specific ticket
- **Duplicate Payment Prevention** - Prevents multiple successful payments for same ticket
- **Helper Method**: `MapToDto()` for cleaner code and reusability

## Step 5: Register Repository in Dependency Injection

### Update `Program.cs`

```csharp
// Register repositories
builder.Services.AddScoped<IStudioRepository, StudioRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
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

#### Create Transaction:
```json
{
  "ticketId": 1,
  "userId": 1,
  "amount": 75000,
  "paymentMethod": 0,
  "paymentStatus": 0,
  "paymentReference": "PAY001"
}
```

#### Update Transaction:
```json
{
  "ticketId": 1,
  "userId": 1,
  "amount": 75000,
  "paymentMethod": 1,
  "paymentStatus": 1,
  "paymentReference": "PAY001-SUCCESS"
}
```

**Enum Values:**

**PaymentMethod:**
- `0` = CreditCard
- `1` = EWallet
- `2` = BankTransfer

**PaymentStatus:**
- `0` = Pending
- `1` = Success
- `2` = Failed

### Expected API Response Structure

```json
{
  "id": 1,
  "ticketId": 1,
  "userId": 1,
  "ticket": {
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
  "amount": 75000,
  "paymentMethod": 0,
  "paymentStatus": 1,
  "transactionDate": "2024-08-15T11:00:00Z",
  "paymentReference": "PAY001-SUCCESS"
}
```

## Project Structure After Implementation

```
MovieApp/
├── Controllers/
│   ├── MoviesController.cs
│   ├── SchedulesController.cs
│   ├── StudiosController.cs
│   ├── TicketsController.cs
│   └── TransactionsController.cs
├── DTOs/
│   ├── MovieDto.cs
│   ├── ScheduleDto.cs
│   ├── StudioDto.cs
│   ├── TicketDto.cs
│   ├── TransactionDto.cs
│   └── UserDto.cs
├── Interfaces/
│   ├── IMovieRepository.cs
│   ├── IScheduleRepository.cs
│   ├── IStudioRepository.cs
│   ├── ITicketRepository.cs
│   └── ITransactionRepository.cs
├── Repositories/
│   ├── MovieRepository.cs
│   ├── ScheduleRepository.cs
│   ├── StudioRepository.cs
│   ├── TicketRepository.cs
│   └── TransactionRepository.cs
├── Models/
│   ├── Movie.cs
│   ├── Schedule.cs
│   ├── Studio.cs
│   ├── Ticket.cs
│   ├── Transaction.cs
│   └── User.cs
├── Data/
│   └── CinemaDbContext.cs
└── Program.cs
```

## Summary

✅ **Created comprehensive Transaction DTOs** with payment validation and complete related entity information
✅ **Created ITransactionRepository interface** with payment business logic validation methods
✅ **Implemented TransactionRepository** with deep include operations and duplicate payment prevention
✅ **Created TransactionsController** with specialized endpoints and business rule validation
✅ **Registered repository** in dependency injection container

**API Endpoints Available:**
- `GET /api/transactions` - Get all transactions with complete context
- `GET /api/transactions/{id}` - Get transaction by ID with full details
- `GET /api/transactions/user/{userId}` - Get user's transaction history
- `GET /api/transactions/ticket/{ticketId}` - Get all transactions for specific ticket
- `POST /api/transactions` - Create new transaction with payment validation
- `PUT /api/transactions/{id}` - Update transaction with duplicate prevention
- `DELETE /api/transactions/{id}` - Delete transaction

**Advanced Business Features:**
- **Duplicate Payment Prevention**: Prevents multiple successful payments for the same ticket
- **Rich Transaction Context**: Complete nested objects for Ticket (with Schedule/Studio/Movie) and User
- **Payment History Tracking**: Specialized endpoints for user and ticket transaction history
- **Automatic Timestamping**: Sets TransactionDate automatically on creation
- **Payment Status Management**: Supports Pending, Success, and Failed payment states
- **Payment Method Support**: Supports CreditCard, EWallet, and BankTransfer methods