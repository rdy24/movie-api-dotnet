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

            // Check if ticket is already paid (for new transactions with Success status)
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0)
            {
                return BadRequest("Invalid transaction ID");
            }

            // Validate if Ticket exists
            if (!await _transactionRepository.TicketExistsAsync(updateTransactionDto.TicketId))
            {
                return BadRequest($"Ticket with ID {updateTransactionDto.TicketId} does not exist");
            }

            // Validate if User exists
            if (!await _transactionRepository.UserExistsAsync(updateTransactionDto.UserId))
            {
                return BadRequest($"User with ID {updateTransactionDto.UserId} does not exist");
            }

            // Check if ticket is already paid by another transaction (excluding current one)
            if (updateTransactionDto.PaymentStatus == PaymentStatus.Success)
            {
                if (await _transactionRepository.TicketAlreadyPaidAsync(updateTransactionDto.TicketId, id))
                {
                    return BadRequest($"Ticket with ID {updateTransactionDto.TicketId} is already paid by another transaction");
                }
            }

            var transaction = new Transaction
            {
                TicketId = updateTransactionDto.TicketId,
                UserId = updateTransactionDto.UserId,
                Amount = updateTransactionDto.Amount,
                PaymentMethod = updateTransactionDto.PaymentMethod,
                PaymentStatus = updateTransactionDto.PaymentStatus,
                PaymentReference = updateTransactionDto.PaymentReference
            };

            var updatedTransaction = await _transactionRepository.UpdateAsync(id, transaction);
            if (updatedTransaction == null)
            {
                return NotFound($"Transaction with ID {id} not found");
            }

            var transactionDto = MapToDto(updatedTransaction);
            return Ok(transactionDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTransaction(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid transaction ID");
            }

            var result = await _transactionRepository.DeleteAsync(id);
            if (!result)
            {
                return NotFound($"Transaction with ID {id} not found");
            }

            return NoContent();
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
                    Id = transaction.Ticket.Id,
                    ScheduleId = transaction.Ticket.ScheduleId,
                    UserId = transaction.Ticket.UserId,
                    Schedule = new ScheduleDto
                    {
                        Id = transaction.Ticket.Schedule.Id,
                        StudioId = transaction.Ticket.Schedule.StudioId,
                        MovieId = transaction.Ticket.Schedule.MovieId,
                        Studio = new StudioDto
                        {
                            Id = transaction.Ticket.Schedule.Studio.Id,
                            Name = transaction.Ticket.Schedule.Studio.Name,
                            Capacity = transaction.Ticket.Schedule.Studio.Capacity,
                            Facilities = transaction.Ticket.Schedule.Studio.Facilities
                        },
                        Movie = new MovieDto
                        {
                            Id = transaction.Ticket.Schedule.Movie.Id,
                            Title = transaction.Ticket.Schedule.Movie.Title,
                            Genre = transaction.Ticket.Schedule.Movie.Genre,
                            Duration = transaction.Ticket.Schedule.Movie.Duration,
                            Description = transaction.Ticket.Schedule.Movie.Description
                        },
                        ShowDateTime = transaction.Ticket.Schedule.ShowDateTime,
                        TicketPrice = transaction.Ticket.Schedule.TicketPrice
                    },
                    User = new UserDto
                    {
                        Id = transaction.Ticket.User.Id,
                        Name = transaction.Ticket.User.Name,
                        Email = transaction.Ticket.User.Email,
                        Username = transaction.Ticket.User.Username,
                        Phone = transaction.Ticket.User.Phone,
                        Role = transaction.Ticket.User.Role,
                        CreatedAt = transaction.Ticket.User.CreatedAt,
                        IsActive = transaction.Ticket.User.IsActive
                    },
                    SeatNumber = transaction.Ticket.SeatNumber,
                    Status = transaction.Ticket.Status,
                    BookedAt = transaction.Ticket.BookedAt
                },
                User = new UserDto
                {
                    Id = transaction.User.Id,
                    Name = transaction.User.Name,
                    Email = transaction.User.Email,
                    Username = transaction.User.Username,
                    Phone = transaction.User.Phone,
                    Role = transaction.User.Role,
                    CreatedAt = transaction.User.CreatedAt,
                    IsActive = transaction.User.IsActive
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