using System.ComponentModel.DataAnnotations;

namespace MovieApp.DTOs
{
    public class ScheduleDto
    {
        public int Id { get; set; }
        public int StudioId { get; set; }
        public int MovieId { get; set; }
        public StudioDto Studio { get; set; } = new();
        public MovieDto Movie { get; set; } = new();
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