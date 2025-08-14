using System.ComponentModel.DataAnnotations;

namespace MovieApp.Models
{
    public enum TicketStatus
    {
        Confirmed,
        Cancelled,
        Expired
    }
    
    public class Ticket
    {
        public int Id { get; set; }
        
        public int ScheduleId { get; set; }
        public virtual Schedule Schedule { get; set; } = null!;
        
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        [Required]
        public string SeatNumber { get; set; } = string.Empty;
        
        public TicketStatus Status { get; set; } = TicketStatus.Confirmed;
        
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual ICollection<Transaction> Transactions { get; set; } = [];
    }
}