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