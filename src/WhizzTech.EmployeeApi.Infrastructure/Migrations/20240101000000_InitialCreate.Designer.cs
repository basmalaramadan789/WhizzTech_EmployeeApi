using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WhizzTech.EmployeeApi.Infrastructure.Persistence;

#nullable disable

namespace WhizzTech.EmployeeApi.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20240101000000_InitialCreate")]
partial class InitialCreate
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("WhizzTech.EmployeeApi.Domain.Entities.Employee", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid").HasColumnName("Id");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");
            b.Property<string>("CreatedBy").IsRequired().HasMaxLength(255).HasColumnType("character varying(255)").HasColumnName("CreatedBy");
            b.Property<string>("CustomData").IsRequired().HasColumnType("jsonb").HasColumnName("CustomData");
            b.Property<DateTime?>("DeletedAt").HasColumnType("timestamp with time zone").HasColumnName("DeletedAt");
            b.Property<string>("Department").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)").HasColumnName("Department");
            b.Property<string>("Email").IsRequired().HasMaxLength(255).HasColumnType("character varying(255)").HasColumnName("Email");
            b.Property<string>("FirstName").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)").HasColumnName("FirstName");
            b.Property<string>("LastName").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)").HasColumnName("LastName");
            b.Property<string>("Status").IsRequired().HasMaxLength(20).HasColumnType("character varying(20)").HasColumnName("Status");
            b.Property<Guid>("TenantId").HasColumnType("uuid").HasColumnName("TenantId");
            b.Property<DateTime>("UpdatedAt").HasColumnType("timestamp with time zone").HasColumnName("UpdatedAt");
            b.Property<string>("UpdatedBy").IsRequired().HasMaxLength(255).HasColumnType("character varying(255)").HasColumnName("UpdatedBy");
            b.HasKey("Id");
            b.HasIndex("TenantId").HasDatabaseName("IX_Employees_TenantId");
            b.HasIndex("TenantId", "Email").IsUnique().HasDatabaseName("IX_Employees_TenantId_Email_Unique").HasFilter("\"DeletedAt\" IS NULL");
            b.ToTable("Employees");
            b.OwnsOne("WhizzTech.EmployeeApi.Domain.ValueObjects.Money", "Salary", b1 =>
            {
                b1.Property<Guid>("EmployeeId").HasColumnType("uuid");
                b1.Property<long>("AmountMinor").HasColumnType("bigint").HasColumnName("SalaryAmountMinor");
                b1.Property<string>("CurrencyCode").IsRequired().HasMaxLength(3).HasColumnType("character varying(3)").HasColumnName("SalaryCurrencyCode");
                b1.HasKey("EmployeeId");
                b1.ToTable("Employees");
                b1.WithOwner().HasForeignKey("EmployeeId");
            });
            b.Navigation("Salary").IsRequired();
        });

        modelBuilder.Entity("WhizzTech.EmployeeApi.Domain.Entities.TaskItem", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid").HasColumnName("Id");
            b.Property<string>("Description").IsRequired().HasMaxLength(1000).HasColumnType("character varying(1000)").HasColumnName("Description");
            b.Property<DateTime>("DueDate").HasColumnType("timestamp with time zone").HasColumnName("DueDate");
            b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)").HasColumnName("Name");
            b.HasKey("Id");
            b.ToTable("Tasks");
        });
#pragma warning restore 612, 618
    }
}
