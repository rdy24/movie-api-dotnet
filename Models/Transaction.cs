using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieApp.Models
{
    public enum PaymentMethod
    {
        CreditCard,
        EWallet,
        BankTransfer
    }
    
    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed
    }
    
    public class Transaction
    {
        public int Id { get; set; }
        
        public int TicketId { get; set; }
        public virtual Ticket Ticket { get; set; } = null!;
        
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        public PaymentMethod PaymentMethod { get; set; }
        
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        public string? PaymentReference { get; set; }
    }
}