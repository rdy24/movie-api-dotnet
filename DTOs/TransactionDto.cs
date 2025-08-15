using System.ComponentModel.DataAnnotations;
using MovieApp.Models;

namespace MovieApp.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public TicketDto Ticket { get; set; } = new();
        public UserDto User { get; set; } = new();
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? PaymentReference { get; set; }
    }

    public class CreateTransactionDto
    {
        [Required(ErrorMessage = "Ticket ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Ticket ID must be a positive number")]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 10000000, ErrorMessage = "Amount must be between 0.01 and 10,000,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [StringLength(100, ErrorMessage = "Payment reference cannot exceed 100 characters")]
        public string? PaymentReference { get; set; }
    }

    public class UpdateTransactionDto
    {
        [Required(ErrorMessage = "Ticket ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Ticket ID must be a positive number")]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 10000000, ErrorMessage = "Amount must be between 0.01 and 10,000,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Payment status is required")]
        public PaymentStatus PaymentStatus { get; set; }

        [StringLength(100, ErrorMessage = "Payment reference cannot exceed 100 characters")]
        public string? PaymentReference { get; set; }
    }
}