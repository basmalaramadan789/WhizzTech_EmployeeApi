using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WhizzTech.EmployeeApi.Domain.Entities;
using WhizzTech.EmployeeApi.Domain.Enums;
using WhizzTech.EmployeeApi.Domain.ValueObjects;
using WhizzTech.EmployeeApi.Infrastructure.Persistence;

namespace WhizzTech.EmployeeApi.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            if (!await context.Employees.AnyAsync())
            {
                var employees = new List<Employee>
                {
                    Employee.Create(TenantAId, "Alice", "Johnson", "alice.johnson@tenanta.com",
                        "Engineering", new Dictionary<string, object?> { ["badge_color"] = "blue", ["clearance_level"] = 3 },
                        new Money(850000, "USD"), "system"),

                    Employee.Create(TenantAId, "Bob", "Smith", "bob.smith@tenanta.com",
                        "HR", new Dictionary<string, object?> { ["badge_color"] = "green", ["clearance_level"] = 1 },
                        new Money(650000, "USD"), "system"),

                    Employee.Create(TenantAId, "Carol", "Williams", "carol.williams@tenanta.com",
                        "Finance", new Dictionary<string, object?> { ["badge_color"] = "red", ["clearance_level"] = 2 },
                        new Money(720000, "USD"), "system"),

                    Employee.Create(TenantBId, "David", "Brown", "david.brown@tenantb.com",
                        "Engineering", new Dictionary<string, object?> { ["office_location"] = "NYC", ["remote"] = true },
                        new Money(900000, "USD"), "system"),

                    Employee.Create(TenantBId, "Eve", "Davis", "eve.davis@tenantb.com",
                        "Marketing", new Dictionary<string, object?> { ["office_location"] = "LA", ["remote"] = false },
                        new Money(700000, "USD"), "system"),
                };

                await context.Employees.AddRangeAsync(employees);
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeded with {Count} employees for 2 tenants.", employees.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
