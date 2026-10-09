using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class RideMatchContext : DbContext
    {
        public RideMatchContext(DbContextOptions<RideMatchContext> options)
          : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Driver> Drivers => Set<Driver>();
        public DbSet<Rider> Riders => Set<Rider>();
        public DbSet<Ride> Rides => Set<Ride>();
        public DbSet<DriverLocation> DriverLocations => Set<DriverLocation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Name).HasMaxLength(150).IsRequired();
                entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
                entity.HasIndex(user => user.Email).IsUnique();
                entity.Property(user => user.PasswordHash).IsRequired();
                entity.Property(user => user.PhoneNumber).HasMaxLength(30).IsRequired();
                entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
                entity.HasOne(user => user.Driver).WithOne(driver => driver.User).HasForeignKey<Driver>(driver => driver.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(user => user.Rider).WithOne(rider => rider.User).HasForeignKey<Rider>(rider => rider.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Driver>(entity =>
            {
                entity.ToTable("Drivers");
                entity.HasKey(driver => driver.Id);
                entity.Property(driver => driver.VehicleType).HasMaxLength(50).IsRequired();
                entity.Property(driver => driver.VehicleNumber).HasMaxLength(30).IsRequired();
                entity.Property(driver => driver.CurrentLatitude).HasPrecision(9, 6);
                entity.Property(driver => driver.CurrentLongitude).HasPrecision(9, 6);
                entity.Property(driver => driver.RowVersion).IsRowVersion();
                entity.HasIndex(driver => new { driver.IsOnline, driver.IsAvailable, driver.LastLocationUpdatedAt });
            });

            modelBuilder.Entity<Rider>(entity =>
            {
                entity.ToTable("Riders");
                entity.HasKey(rider => rider.Id);
            });

            modelBuilder.Entity<Ride>(entity =>
            {
                entity.ToTable("Rides");
                entity.HasKey(ride => ride.Id);
                entity.Property(ride => ride.PickupLatitude).HasPrecision(9, 6).IsRequired();
                entity.Property(ride => ride.PickupLongitude).HasPrecision(9, 6).IsRequired();
                entity.Property(ride => ride.DestinationLatitude).HasPrecision(9, 6).IsRequired();
                entity.Property(ride => ride.DestinationLongitude).HasPrecision(9, 6).IsRequired();
                entity.Property(ride => ride.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
                entity.Property(ride => ride.Fare).HasPrecision(18, 2);
                entity.Property(ride => ride.RowVersion).IsRowVersion();
                entity.HasIndex(ride => new { ride.RiderId, ride.RequestedAt });
                entity.HasIndex(ride => new { ride.DriverId, ride.Status });
                entity.HasOne(ride => ride.Rider).WithMany(rider => rider.Rides).HasForeignKey(ride => ride.RiderId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ride => ride.Driver).WithMany(driver => driver.Rides).HasForeignKey(ride => ride.DriverId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DriverLocation>(entity =>
            {
                entity.ToTable("DriverLocations");
                entity.HasKey(location => location.Id);
                entity.Property(location => location.Latitude).HasPrecision(9, 6).IsRequired();
                entity.Property(location => location.Longitude).HasPrecision(9, 6).IsRequired();
                entity.HasIndex(location => new { location.DriverId, location.RecordedAt });
                entity.HasOne(location => location.Driver).WithMany(driver => driver.Locations).HasForeignKey(location => location.DriverId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
