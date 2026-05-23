using MediatR;
using WhizzTech.EmployeeApi.Application.Common.DTOs;
using WhizzTech.EmployeeApi.Application.Common.Mappings;
using WhizzTech.EmployeeApi.Domain.Exceptions;
using WhizzTech.EmployeeApi.Domain.Repositories;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Queries.GetEmployeeById;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, ApiResponse<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeeByIdQueryHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<EmployeeDto>> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _repository.GetByIdAsync(request.Id, request.TenantId, cancellationToken)
            ?? throw new EmployeeNotFoundException(request.Id);

        return ApiResponse<EmployeeDto>.Success(employee.ToDto());
    }
}
