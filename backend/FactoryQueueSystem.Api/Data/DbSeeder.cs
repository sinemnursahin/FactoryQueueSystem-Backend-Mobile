using FactoryQueueSystem.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var config = services.GetRequiredService<IConfiguration>();
        var hasher = new PasswordHasher<User>();
        var now = DateTime.UtcNow;
        var adminEmail = config["DemoCredentials:AdminEmail"] ?? "admin@factoryqueue.local";
        var adminPassword = config["DemoCredentials:AdminPassword"] ?? "Admin123!";
        var driverEmail = config["DemoCredentials:DriverEmail"] ?? "driver@factoryqueue.local";
        var driverPassword = config["DemoCredentials:DriverPassword"] ?? "Driver123!";

        var admin = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == adminEmail);
        if (admin == null)
        {
            admin = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Demo",
                LastName = "Admin",
                Email = adminEmail,
                Role = "Admin",
                CreatedAt = now
            };
            admin.PasswordHash = hasher.HashPassword(admin, adminPassword);
            db.Users.Add(admin);
        }
        else
        {
            admin.IsDeleted = false;
            admin.DeletedAt = null;
        }

        var driver = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == driverEmail);
        if (driver == null)
        {
            driver = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Demo",
                LastName = "Driver",
                Email = driverEmail,
                Role = "Driver",
                CreatedAt = now
            };
            driver.PasswordHash = hasher.HashPassword(driver, driverPassword);
            db.Users.Add(driver);
        }
        else
        {
            driver.IsDeleted = false;
            driver.DeletedAt = null;
        }

        await db.SaveChangesAsync();

        var vehicle = await db.Vehicles.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.PlateNumber == "34 FQS 001");
        if (vehicle == null)
        {
            vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                PlateNumber = "34 FQS 001",
                UserId = driver.Id,
                DriverName = "Demo Driver",
                CreatedAt = now
            };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync();
        }
        else
        {
            vehicle.IsDeleted = false;
            vehicle.DeletedAt = null;
            await db.SaveChangesAsync();
        }

        var hasActiveShipment = await db.Shipments
            .AnyAsync(x => (x.UserId == driver.Id || x.VehicleId == vehicle.Id) && x.Status != ShipmentStatus.Completed);
        if (!hasActiveShipment)
        {
            db.Shipments.Add(new Shipment
            {
                Id = Guid.NewGuid(),
                UserId = driver.Id,
                Status = ShipmentStatus.OnTheWay,
                RawMaterialName = "Demo Raw Material",
                SupplierName = "Demo Supplier",
                CreatedAt = now
            });
            await db.SaveChangesAsync();
        }
    }

    public static async Task ResetDemoAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var config = services.GetRequiredService<IConfiguration>();
        var hasher = new PasswordHasher<User>();
        var now = DateTime.UtcNow;
        var adminEmail = config["DemoCredentials:AdminEmail"] ?? "admin@factoryqueue.local";
        var adminPassword = config["DemoCredentials:AdminPassword"] ?? "Admin123!";
        var driverEmail = config["DemoCredentials:DriverEmail"] ?? "driver@factoryqueue.local";
        var driverPassword = config["DemoCredentials:DriverPassword"] ?? "Driver123!";

        await db.WeighingRecords.ExecuteDeleteAsync();
        await db.Shipments.ExecuteDeleteAsync();
        await db.Vehicles.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Users.IgnoreQueryFilters()
            .Where(x => x.Email != adminEmail && x.Email != driverEmail)
            .ExecuteDeleteAsync();

        var admin = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == adminEmail);
        if (admin == null)
        {
            admin = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Demo",
                LastName = "Admin",
                Email = adminEmail,
                Role = "Admin",
                CreatedAt = now
            };
            admin.PasswordHash = hasher.HashPassword(admin, adminPassword);
            db.Users.Add(admin);
        }
        else
        {
            admin.IsDeleted = false;
            admin.DeletedAt = null;
        }

        var driver = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == driverEmail);
        if (driver == null)
        {
            driver = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Demo",
                LastName = "Driver",
                Email = driverEmail,
                Role = "Driver",
                CreatedAt = now
            };
            driver.PasswordHash = hasher.HashPassword(driver, driverPassword);
            db.Users.Add(driver);
        }
        else
        {
            driver.IsDeleted = false;
            driver.DeletedAt = null;
        }

        await db.SaveChangesAsync();

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            PlateNumber = "34 FQS 001",
            UserId = driver.Id,
            DriverName = "Demo Driver",
            CreatedAt = now
        };

        db.Vehicles.Add(vehicle);
        db.Shipments.Add(new Shipment
        {
            Id = Guid.NewGuid(),
            UserId = driver.Id,
            Status = ShipmentStatus.OnTheWay,
            RawMaterialName = "Demo Raw Material",
            SupplierName = "Demo Supplier",
            CreatedAt = now
        });

        await db.SaveChangesAsync();
    }
}
