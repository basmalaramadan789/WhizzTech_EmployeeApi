using MediatR;
using WhizzTech.EmployeeApi.Application.Common.DTOs;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Queries.ListEmployees;

public record ListEmployeesQuery(
    Guid TenantId,
    int Page,
    int PageSize,
    string? Department,
    string? Status,
    string? SearchTerm
) : IRequest<ApiResponse<IReadOnlyList<EmployeeDto>>>;
