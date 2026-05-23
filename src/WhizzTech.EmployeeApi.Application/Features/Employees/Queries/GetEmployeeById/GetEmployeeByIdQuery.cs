using MediatR;
using WhizzTech.EmployeeApi.Application.Common.DTOs;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id, Guid TenantId) : IRequest<ApiResponse<EmployeeDto>>;
