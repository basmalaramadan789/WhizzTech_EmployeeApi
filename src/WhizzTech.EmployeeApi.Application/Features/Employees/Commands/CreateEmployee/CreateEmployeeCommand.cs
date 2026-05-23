using MediatR;
using WhizzTech.EmployeeApi.Application.Common.DTOs;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    Guid TenantId,
    string FirstName,
    string LastName,
    string Email,
    string Department,
    Dictionary<string, object?>? CustomData,
    long? SalaryAmountMinor,
    string? SalaryCurrencyCode,
    string CreatedBy
) : IRequest<ApiResponse<EmployeeDto>>;
