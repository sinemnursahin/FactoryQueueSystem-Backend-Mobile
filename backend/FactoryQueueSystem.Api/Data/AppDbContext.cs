using FactoryQueueSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<WeighingRecord> WeighingRecords => Set<WeighingRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.PhoneNumber).HasMaxLength(30);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            entity.HasIndex(x => x.PhoneNumber).IsUnique().HasFilter("[PhoneNumber] IS NOT NULL");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.Property(x => x.PlateNumber).HasMaxLength(20).IsRequired();
            entity.Property(x => x.DriverName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.PlateNumber).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasOne(x => x.User).WithMany(x => x.Vehicles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<int>().IsRequired();
            entity.Property(x => x.RawMaterialName).HasMaxLength(150);
            entity.Property(x => x.SupplierName).HasMaxLength(150);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.VehicleId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => new { x.QueueDate, x.QueueNumber })
                .IsUnique()
                .HasFilter("[QueueDate] IS NOT NULL AND [QueueNumber] IS NOT NULL");
            entity.HasOne(x => x.Vehicle).WithMany(x => x.Shipments).HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WeighingRecord>(entity =>
        {
            entity.Property(x => x.LoadedWeight).HasPrecision(18, 2);
            entity.Property(x => x.EmptyWeight).HasPrecision(18, 2);
            entity.Property(x => x.NetAmount).HasPrecision(18, 2);
            entity.HasIndex(x => x.ShipmentId).IsUnique();
            entity.HasOne(x => x.Shipment).WithOne(x => x.WeighingRecord).HasForeignKey<WeighingRecord>(x => x.ShipmentId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
