using MediatR;
using WhizzTech.EmployeeApi.Application.Common.DTOs;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(
    Guid Id,
    Guid TenantId,
    string DeletedBy
) : IRequest<ApiResponse<bool>>;
