using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhizzTech.EmployeeApi.Domain.Entities;
using WhizzTech.EmployeeApi.Domain.Enums;

namespace WhizzTech.EmployeeApi.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("TenantId")
            .IsRequired();

        builder.Property(e => e.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasColumnName("LastName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasColumnName("Email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Department)
            .HasColumnName("Department")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.CustomData)
            .HasColumnName("CustomData")
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(v,
                    (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object?>());

        builder.OwnsOne(e => e.Salary, salary =>
        {
            salary.Property(s => s.AmountMinor)
                .HasColumnName("SalaryAmountMinor")
                .IsRequired();
            salary.Property(s => s.CurrencyCode)
                .HasColumnName("SalaryCurrencyCode")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
        builder.Property(e => e.CreatedBy).HasColumnName("CreatedBy").HasMaxLength(255).IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UpdatedBy").HasMaxLength(255).IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.Email })
            .HasFilter("\"DeletedAt\" IS NULL")
            .IsUnique()
            .HasDatabaseName("IX_Employees_TenantId_Email_Unique");

        builder.HasIndex(e => e.TenantId).HasDatabaseName("IX_Employees_TenantId");

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
