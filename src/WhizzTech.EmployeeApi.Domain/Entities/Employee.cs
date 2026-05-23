using WhizzTech.EmployeeApi.Domain.Enums;
using WhizzTech.EmployeeApi.Domain.ValueObjects;

namespace WhizzTech.EmployeeApi.Domain.Entities;

public class Employee
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public EmployeeStatus Status { get; private set; }
    public Dictionary<string, object?> CustomData { get; private set; } = new();
    public Money Salary { get; private set; } = new(0, "USD");
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;
    public string UpdatedBy { get; private set; } = string.Empty;

    private Employee() { } 

    public static Employee Create(
        Guid tenantId,
        string firstName,
        string lastName,
        string email,
        string department,
        Dictionary<string, object?>? customData,
        Money? salary,
        string createdBy)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            Department = department,
            Status = EmployeeStatus.Active,
            CustomData = customData ?? new(),
            Salary = salary ?? new Money(0, "USD"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void Update(
        string firstName,
        string lastName,
        string email,
        string department,
        EmployeeStatus status,
        Dictionary<string, object?>? customData,
        Money? salary,
        string updatedBy)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email.ToLowerInvariant();
        Department = department;
        Status = status;
        CustomData = customData ?? new();
        if (salary is not null) Salary = salary;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void SoftDelete(string deletedBy)
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
    }

    public bool IsDeleted => DeletedAt.HasValue;
}
