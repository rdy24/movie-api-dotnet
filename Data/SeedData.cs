using MovieApp.Models;
using Microsoft.EntityFrameworkCore;
using MovieApp.Utils;

namespace MovieApp.Data
{
    public static class SeedData
    {
        public static void Seed(CinemaDbContext context)
        {
            context.Database.EnsureCreated();
            
            // Seed Movies
            if (!context.Movies.Any())
            {
                context.Movies.AddRange(
                    new Movie { Title = "Avengers: Endgame", Genre = "Action", Duration = 181, Description = "The epic conclusion to the Infinity Saga" },
                    new Movie { Title = "Parasite", Genre = "Thriller", Duration = 132, Description = "A dark comedy thriller about class conflict" },
                    new Movie { Title = "Spider-Man: No Way Home", Genre = "Action", Duration = 148, Description = "Peter Parker's multiverse adventure" }
                );
                context.SaveChanges();
            }

            // Seed Studios
            if (!context.Studios.Any())
            {
                context.Studios.AddRange(
                    new Studio { Name = "Studio IMAX", Capacity = 200, Facilities = "IMAX Screen, Dolby Atmos" },
                    new Studio { Name = "Studio Premium", Capacity = 150, Facilities = "Reclining Seats, 4K Projection" },
                    new Studio { Name = "Studio Regular", Capacity = 100, Facilities = "Standard Screen, Surround Sound" }
                );
                context.SaveChanges();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User { 
                        Name = "John Doe", 
                        Email = "john.doe@email.com", 
                        Username = "johndoe", 
                        Password = PasswordHelper.HashPassword("123456"),
                        Phone = "081234567890",
                        Role = "Customer",
                        CreatedAt = new DateTime(2024, 1, 1, 9, 0, 0),
                        IsActive = true
                    },
                    new User { 
                        Name = "Jane Smith", 
                        Email = "jane.smith@email.com", 
                        Username = "janesmith", 
                        Password = PasswordHelper.HashPassword("123456"),
                        Phone = "081234567891",
                        Role = "Customer",
                        CreatedAt = new DateTime(2024, 1, 2, 10, 0, 0),
                        IsActive = true
                    },
                    new User { 
                        Name = "Bob Wilson", 
                        Email = "bob.wilson@email.com", 
                        Username = "bobwilson", 
                        Password = PasswordHelper.HashPassword("123456"),
                        Phone = "081234567892",
                        Role = "Customer",
                        CreatedAt = new DateTime(2024, 1, 3, 11, 0, 0),
                        IsActive = true
                    },
                    new User { 
                        Name = "Admin User", 
                        Email = "admin@cinema.com", 
                        Username = "admin", 
                        Password = PasswordHelper.HashPassword("admin123"),
                        Phone = "081234567999",
                        Role = "Admin",
                        CreatedAt = new DateTime(2024, 1, 1, 8, 0, 0),
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            // Seed Schedules - depends on Movies and Studios
            if (!context.Schedules.Any())
            {
                var movies = context.Movies.ToList();
                var studios = context.Studios.ToList();
                
                if (movies.Count >= 3 && studios.Count >= 3)
                {
                    context.Schedules.AddRange(
                        new Schedule { StudioId = studios[0].Id, MovieId = movies[0].Id, ShowDateTime = new DateTime(2024, 1, 15, 19, 0, 0), TicketPrice = 75000 },
                        new Schedule { StudioId = studios[1].Id, MovieId = movies[1].Id, ShowDateTime = new DateTime(2024, 1, 15, 21, 30, 0), TicketPrice = 60000 },
                        new Schedule { StudioId = studios[2].Id, MovieId = movies[2].Id, ShowDateTime = new DateTime(2024, 1, 16, 14, 0, 0), TicketPrice = 50000 }
                    );
                    context.SaveChanges();
                }
            }

            // Seed Tickets - depends on Schedules and Users
            if (!context.Tickets.Any())
            {
                var schedules = context.Schedules.ToList();
                var users = context.Users.Where(u => u.Role == "Customer").ToList();
                
                if (schedules.Count >= 3 && users.Count >= 3)
                {
                    context.Tickets.AddRange(
                        new Ticket { ScheduleId = schedules[0].Id, UserId = users[0].Id, SeatNumber = "A1", Status = TicketStatus.Confirmed, BookedAt = new DateTime(2024, 1, 10, 10, 0, 0) },
                        new Ticket { ScheduleId = schedules[1].Id, UserId = users[1].Id, SeatNumber = "B5", Status = TicketStatus.Confirmed, BookedAt = new DateTime(2024, 1, 11, 15, 30, 0) },
                        new Ticket { ScheduleId = schedules[2].Id, UserId = users[2].Id, SeatNumber = "C3", Status = TicketStatus.Cancelled, BookedAt = new DateTime(2024, 1, 12, 9, 15, 0) }
                    );
                    context.SaveChanges();
                }
            }

            // Seed Transactions - depends on Tickets and Users
            if (!context.Transactions.Any())
            {
                var tickets = context.Tickets.ToList();
                var users = context.Users.Where(u => u.Role == "Customer").ToList();
                
                if (tickets.Count >= 3 && users.Count >= 3)
                {
                    context.Transactions.AddRange(
                        new Transaction { TicketId = tickets[0].Id, UserId = users[0].Id, Amount = 75000, PaymentMethod = PaymentMethod.CreditCard, PaymentStatus = PaymentStatus.Success, TransactionDate = new DateTime(2024, 1, 10, 10, 5, 0), PaymentReference = "TXN001" },
                        new Transaction { TicketId = tickets[1].Id, UserId = users[1].Id, Amount = 60000, PaymentMethod = PaymentMethod.EWallet, PaymentStatus = PaymentStatus.Success, TransactionDate = new DateTime(2024, 1, 11, 15, 35, 0), PaymentReference = "TXN002" },
                        new Transaction { TicketId = tickets[2].Id, UserId = users[2].Id, Amount = 50000, PaymentMethod = PaymentMethod.BankTransfer, PaymentStatus = PaymentStatus.Failed, TransactionDate = new DateTime(2024, 1, 12, 9, 20, 0), PaymentReference = "TXN003" }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}