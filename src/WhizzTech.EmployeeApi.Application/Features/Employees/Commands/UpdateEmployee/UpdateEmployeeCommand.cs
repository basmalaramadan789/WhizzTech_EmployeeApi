using MediatR;
using WhizzTech.EmployeeApi.Application.Common.DTOs;
using WhizzTech.EmployeeApi.Domain.Enums;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(
    Guid Id,
    Guid TenantId,
    string FirstName,
    string LastName,
    string Email,
    string Department,
    EmployeeStatus Status,
    Dictionary<string, object?>? CustomData,
    long? SalaryAmountMinor,
    string? SalaryCurrencyCode,
    string UpdatedBy
) : IRequest<ApiResponse<EmployeeDto>>;
