using System.ComponentModel.DataAnnotations;

namespace MovieApp.Models
{
    public class Studio
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public int Capacity { get; set; }
        
        public string? Facilities { get; set; }
        
        // Navigation property
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
