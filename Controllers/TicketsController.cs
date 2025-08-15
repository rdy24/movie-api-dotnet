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

            var ticketDto = new TicketDto
            {
                Id = ticket.Id,
                ScheduleId = ticket.ScheduleId,
                UserId = ticket.UserId,
                Schedule = new ScheduleDto
                {
                    Id = ticket.Schedule.Id,
                    StudioId = ticket.Schedule.StudioId,
                    MovieId = ticket.Schedule.MovieId,
                    Studio = new StudioDto
                    {
                        Id = ticket.Schedule.Studio.Id,
                        Name = ticket.Schedule.Studio.Name,
                        Capacity = ticket.Schedule.Studio.Capacity,
                        Facilities = ticket.Schedule.Studio.Facilities
                    },
                    Movie = new MovieDto
                    {
                        Id = ticket.Schedule.Movie.Id,
                        Title = ticket.Schedule.Movie.Title,
                        Genre = ticket.Schedule.Movie.Genre,
                        Duration = ticket.Schedule.Movie.Duration,
                        Description = ticket.Schedule.Movie.Description
                    },
                    ShowDateTime = ticket.Schedule.ShowDateTime,
                    TicketPrice = ticket.Schedule.TicketPrice
                },
                User = new UserDto
                {
                    Id = ticket.User.Id,
                    Name = ticket.User.Name,
                    Email = ticket.User.Email,
                    Username = ticket.User.Username,
                    Phone = ticket.User.Phone,
                    Role = ticket.User.Role,
                    CreatedAt = ticket.User.CreatedAt,
                    IsActive = ticket.User.IsActive
                },
                SeatNumber = ticket.SeatNumber,
                Status = ticket.Status,
                BookedAt = ticket.BookedAt
            };

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

            var ticketDto = new TicketDto
            {
                Id = createdTicket.Id,
                ScheduleId = createdTicket.ScheduleId,
                UserId = createdTicket.UserId,
                Schedule = new ScheduleDto
                {
                    Id = createdTicket.Schedule.Id,
                    StudioId = createdTicket.Schedule.StudioId,
                    MovieId = createdTicket.Schedule.MovieId,
                    Studio = new StudioDto
                    {
                        Id = createdTicket.Schedule.Studio.Id,
                        Name = createdTicket.Schedule.Studio.Name,
                        Capacity = createdTicket.Schedule.Studio.Capacity,
                        Facilities = createdTicket.Schedule.Studio.Facilities
                    },
                    Movie = new MovieDto
                    {
                        Id = createdTicket.Schedule.Movie.Id,
                        Title = createdTicket.Schedule.Movie.Title,
                        Genre = createdTicket.Schedule.Movie.Genre,
                        Duration = createdTicket.Schedule.Movie.Duration,
                        Description = createdTicket.Schedule.Movie.Description
                    },
                    ShowDateTime = createdTicket.Schedule.ShowDateTime,
                    TicketPrice = createdTicket.Schedule.TicketPrice
                },
                User = new UserDto
                {
                    Id = createdTicket.User.Id,
                    Name = createdTicket.User.Name,
                    Email = createdTicket.User.Email,
                    Username = createdTicket.User.Username,
                    Phone = createdTicket.User.Phone,
                    Role = createdTicket.User.Role,
                    CreatedAt = createdTicket.User.CreatedAt,
                    IsActive = createdTicket.User.IsActive
                },
                SeatNumber = createdTicket.SeatNumber,
                Status = createdTicket.Status,
                BookedAt = createdTicket.BookedAt
            };

            return CreatedAtAction(nameof(GetTicket), new { id = ticketDto.Id }, ticketDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TicketDto>> UpdateTicket(int id, UpdateTicketDto updateTicketDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0)
            {
                return BadRequest("Invalid ticket ID");
            }

            // Validate if Schedule exists
            if (!await _ticketRepository.ScheduleExistsAsync(updateTicketDto.ScheduleId))
            {
                return BadRequest($"Schedule with ID {updateTicketDto.ScheduleId} does not exist");
            }

            // Validate if User exists
            if (!await _ticketRepository.UserExistsAsync(updateTicketDto.UserId))
            {
                return BadRequest($"User with ID {updateTicketDto.UserId} does not exist");
            }

            // Validate seat availability (excluding current ticket)
            if (!await _ticketRepository.SeatAvailableAsync(updateTicketDto.ScheduleId, updateTicketDto.SeatNumber, id))
            {
                return BadRequest($"Seat {updateTicketDto.SeatNumber} is already booked for this schedule");
            }

            var ticket = new Ticket
            {
                ScheduleId = updateTicketDto.ScheduleId,
                UserId = updateTicketDto.UserId,
                SeatNumber = updateTicketDto.SeatNumber,
                Status = updateTicketDto.Status
            };

            var updatedTicket = await _ticketRepository.UpdateAsync(id, ticket);
            if (updatedTicket == null)
            {
                return NotFound($"Ticket with ID {id} not found");
            }

            var ticketDto = new TicketDto
            {
                Id = updatedTicket.Id,
                ScheduleId = updatedTicket.ScheduleId,
                UserId = updatedTicket.UserId,
                Schedule = new ScheduleDto
                {
                    Id = updatedTicket.Schedule.Id,
                    StudioId = updatedTicket.Schedule.StudioId,
                    MovieId = updatedTicket.Schedule.MovieId,
                    Studio = new StudioDto
                    {
                        Id = updatedTicket.Schedule.Studio.Id,
                        Name = updatedTicket.Schedule.Studio.Name,
                        Capacity = updatedTicket.Schedule.Studio.Capacity,
                        Facilities = updatedTicket.Schedule.Studio.Facilities
                    },
                    Movie = new MovieDto
                    {
                        Id = updatedTicket.Schedule.Movie.Id,
                        Title = updatedTicket.Schedule.Movie.Title,
                        Genre = updatedTicket.Schedule.Movie.Genre,
                        Duration = updatedTicket.Schedule.Movie.Duration,
                        Description = updatedTicket.Schedule.Movie.Description
                    },
                    ShowDateTime = updatedTicket.Schedule.ShowDateTime,
                    TicketPrice = updatedTicket.Schedule.TicketPrice
                },
                User = new UserDto
                {
                    Id = updatedTicket.User.Id,
                    Name = updatedTicket.User.Name,
                    Email = updatedTicket.User.Email,
                    Username = updatedTicket.User.Username,
                    Phone = updatedTicket.User.Phone,
                    Role = updatedTicket.User.Role,
                    CreatedAt = updatedTicket.User.CreatedAt,
                    IsActive = updatedTicket.User.IsActive
                },
                SeatNumber = updatedTicket.SeatNumber,
                Status = updatedTicket.Status,
                BookedAt = updatedTicket.BookedAt
            };

            return Ok(ticketDto);
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