using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieApp.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        
        public int StudioId { get; set; }
        public virtual Studio Studio { get; set; } = null!;
        
        public int MovieId { get; set; }
        public virtual Movie Movie { get; set; } = null!;
        
        public DateTime ShowDateTime { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TicketPrice { get; set; }
        
        // Navigation property
        public virtual ICollection<Ticket> Tickets { get; set; } = [];
    }
}