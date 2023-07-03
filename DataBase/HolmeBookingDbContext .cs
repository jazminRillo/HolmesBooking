using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HolmesBooking.DataBase
{
    public class HolmeBookingDbContext : DbContext
    {
        public HolmeBookingDbContext(DbContextOptions<HolmeBookingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(s => s.Schedule)
                    .HasColumnType("nvarchar(max)")
                    .HasConversion(
                        schedule => JsonConvert.SerializeObject(schedule),
                        json => JsonConvert.DeserializeObject<Dictionary<DayOfWeek, List<TimeSpan>>>(json)
                    );
            });
        }
    }  
        
    
}
