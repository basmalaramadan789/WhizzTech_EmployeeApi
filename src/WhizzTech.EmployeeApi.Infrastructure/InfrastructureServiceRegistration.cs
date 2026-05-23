using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WhizzTech.EmployeeApi.Application.Common.CustomData;
using WhizzTech.EmployeeApi.Domain.Repositories;
using WhizzTech.EmployeeApi.Infrastructure.Persistence;
using WhizzTech.EmployeeApi.Infrastructure.Repositories;

namespace WhizzTech.EmployeeApi.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        RegisterTenantCustomFields();

        return services;
    }

    private static void RegisterTenantCustomFields()
    {
        TenantCustomFieldRegistry.Register(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            new List<CustomFieldDefinition>
            {
                new("badge_color", "string", IsRequired: true),
                new("clearance_level", "number", IsRequired: true),
                new("notes", "string", IsRequired: false)
            });

        TenantCustomFieldRegistry.Register(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            new List<CustomFieldDefinition>
            {
                new("office_location", "string", IsRequired: true),
                new("remote", "boolean", IsRequired: true),
                new("team_name", "string", IsRequired: false)
            });
    }
}
