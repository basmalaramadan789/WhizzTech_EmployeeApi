using MediatR;
using WhizzTech.EmployeeApi.Application.Common.CustomData;
using WhizzTech.EmployeeApi.Application.Common.DTOs;
using WhizzTech.EmployeeApi.Application.Common.Mappings;
using WhizzTech.EmployeeApi.Domain.Exceptions;
using WhizzTech.EmployeeApi.Domain.Repositories;
using WhizzTech.EmployeeApi.Domain.ValueObjects;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, ApiResponse<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;

    public UpdateEmployeeCommandHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<EmployeeDto>> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _repository.GetByIdAsync(request.Id, request.TenantId, cancellationToken)
            ?? throw new EmployeeNotFoundException(request.Id);

        if (request.CustomData is not null)
            CustomDataValidator.Validate(request.TenantId, request.CustomData);

        var emailExists = await _repository.ExistsByEmailAsync(
            request.Email, request.TenantId, excludeId: request.Id, cancellationToken);

        if (emailExists)
            throw new DuplicateEmailException(request.Email);

        Money? salary = null;
        if (request.SalaryAmountMinor.HasValue && request.SalaryCurrencyCode is not null)
            salary = new Money(request.SalaryAmountMinor.Value, request.SalaryCurrencyCode);

        employee.Update(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            department: request.Department,
            status: request.Status,
            customData: request.CustomData,
            salary: salary,
            updatedBy: request.UpdatedBy);

        await _repository.UpdateAsync(employee, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ApiResponse<EmployeeDto>.Success(employee.ToDto());
    }
}
