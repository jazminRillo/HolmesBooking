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
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<DatesNotAvailable> DatesNotAvailable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(s => s.Schedule)
                    .HasColumnType("jsonb")
                    .HasConversion(
                        schedule => JsonConvert.SerializeObject(schedule),
                        json => JsonConvert.DeserializeObject<Dictionary<int, List<TimeSpan>>>(json)!
                    );
            });

            modelBuilder.Entity<Customer>()
                    .Property(c => c.Classification)
                    .HasConversion(
                        v => v.ToString(),  // Convertir el valor del Enum a cadena de texto
                        v => (Classification)Enum.Parse(typeof(Classification), v!)  // Convertir la cadena de texto al Enum correspondiente
                    );

            modelBuilder.Entity<Reservation>()
                    .Property(c => c.State)
                    .HasConversion(
                        v => v.ToString(),  // Convertir el valor del Enum a cadena de texto
                        v => (State)Enum.Parse(typeof(State), v!)  // Convertir la cadena de texto al Enum correspondiente
                    );

            modelBuilder.Entity<UserRoles>()
                    .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRoles>()
                    .HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRoles>()
                    .HasOne(ur => ur.Role);
        }

    }


}
