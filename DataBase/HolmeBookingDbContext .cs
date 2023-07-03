using Microsoft.EntityFrameworkCore;

namespace HolmesBooking.DataBase
{
    public class HolmeBookingDbContext : DbContext
    {
        public DbSet<Service> Services { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
    }
}
