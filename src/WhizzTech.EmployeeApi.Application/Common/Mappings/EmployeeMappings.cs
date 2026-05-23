using WhizzTech.EmployeeApi.Domain.Entities;
using WhizzTech.EmployeeApi.Application.Common.DTOs;

namespace WhizzTech.EmployeeApi.Application.Common.Mappings;

public static class EmployeeMappings
{
    public static EmployeeDto ToDto(this Employee e) => new(
        Id: e.Id,
        TenantId: e.TenantId,
        FirstName: e.FirstName,
        LastName: e.LastName,
        Email: e.Email,
        Department: e.Department,
        Status: e.Status.ToString(),
        CustomData: e.CustomData,
        Salary: new SalaryDto(e.Salary.AmountMinor, e.Salary.CurrencyCode, e.Salary.ToMajorUnit()),
        CreatedBy: e.CreatedBy,
        UpdatedBy: e.UpdatedBy,
        CreatedAt: e.CreatedAt,
        UpdatedAt: e.UpdatedAt
    );
}
