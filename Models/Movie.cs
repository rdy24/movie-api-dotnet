using System.ComponentModel.DataAnnotations;

namespace MovieApp.Models
{
    public class Movie
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Genre { get; set; }

        public int Duration { get; set; } = 0;

        public string? Description { get; set; }
    }
}