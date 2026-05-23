using MediatR;
using WhizzTech.EmployeeApi.Application.Common.DTOs;
using WhizzTech.EmployeeApi.Domain.Exceptions;
using WhizzTech.EmployeeApi.Domain.Repositories;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Commands.DeleteEmployee;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, ApiResponse<bool>>
{
    private readonly IEmployeeRepository _repository;

    public DeleteEmployeeCommandHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _repository.GetByIdAsync(request.Id, request.TenantId, cancellationToken)
            ?? throw new EmployeeNotFoundException(request.Id);

        employee.SoftDelete(request.DeletedBy);

        await _repository.UpdateAsync(employee, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true);
    }
}
