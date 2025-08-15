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