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
                MovieId = s.MovieId,
                Studio = new StudioDto
                {
                    Id = s.Studio.Id,
                    Name = s.Studio.Name,
                    Capacity = s.Studio.Capacity,
                    Facilities = s.Studio.Facilities
                },
                Movie = new MovieDto
                {
                    Id = s.Movie.Id,
                    Title = s.Movie.Title,
                    Genre = s.Movie.Genre,
                    Duration = s.Movie.Duration,
                    Description = s.Movie.Description
                },
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
                MovieId = schedule.MovieId,
                Studio = new StudioDto
                {
                    Id = schedule.Studio.Id,
                    Name = schedule.Studio.Name,
                    Capacity = schedule.Studio.Capacity,
                    Facilities = schedule.Studio.Facilities
                },
                Movie = new MovieDto
                {
                    Id = schedule.Movie.Id,
                    Title = schedule.Movie.Title,
                    Genre = schedule.Movie.Genre,
                    Duration = schedule.Movie.Duration,
                    Description = schedule.Movie.Description
                },
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
                MovieId = createdSchedule.MovieId,
                Studio = new StudioDto
                {
                    Id = createdSchedule.Studio.Id,
                    Name = createdSchedule.Studio.Name,
                    Capacity = createdSchedule.Studio.Capacity,
                    Facilities = createdSchedule.Studio.Facilities
                },
                Movie = new MovieDto
                {
                    Id = createdSchedule.Movie.Id,
                    Title = createdSchedule.Movie.Title,
                    Genre = createdSchedule.Movie.Genre,
                    Duration = createdSchedule.Movie.Duration,
                    Description = createdSchedule.Movie.Description
                },
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
                MovieId = updatedSchedule.MovieId,
                Studio = new StudioDto
                {
                    Id = updatedSchedule.Studio.Id,
                    Name = updatedSchedule.Studio.Name,
                    Capacity = updatedSchedule.Studio.Capacity,
                    Facilities = updatedSchedule.Studio.Facilities
                },
                Movie = new MovieDto
                {
                    Id = updatedSchedule.Movie.Id,
                    Title = updatedSchedule.Movie.Title,
                    Genre = updatedSchedule.Movie.Genre,
                    Duration = updatedSchedule.Movie.Duration,
                    Description = updatedSchedule.Movie.Description
                },
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