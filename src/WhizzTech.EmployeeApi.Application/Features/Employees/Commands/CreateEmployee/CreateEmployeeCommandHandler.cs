using MediatR;
using WhizzTech.EmployeeApi.Application.Common.CustomData;
using WhizzTech.EmployeeApi.Application.Common.DTOs;
using WhizzTech.EmployeeApi.Application.Common.Mappings;
using WhizzTech.EmployeeApi.Domain.Entities;
using WhizzTech.EmployeeApi.Domain.Exceptions;
using WhizzTech.EmployeeApi.Domain.Repositories;
using WhizzTech.EmployeeApi.Domain.ValueObjects;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, ApiResponse<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;

    public CreateEmployeeCommandHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<EmployeeDto>> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CustomData is not null)
            CustomDataValidator.Validate(request.TenantId, request.CustomData);

        var emailExists = await _repository.ExistsByEmailAsync(
            request.Email, request.TenantId, null, cancellationToken);

        if (emailExists)
            throw new DuplicateEmailException(request.Email);

        Money? salary = null;
        if (request.SalaryAmountMinor.HasValue && request.SalaryCurrencyCode is not null)
            salary = new Money(request.SalaryAmountMinor.Value, request.SalaryCurrencyCode);

        var employee = Employee.Create(
            tenantId: request.TenantId,
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            department: request.Department,
            customData: request.CustomData,
            salary: salary,
            createdBy: request.CreatedBy);

        await _repository.AddAsync(employee, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ApiResponse<EmployeeDto>.Success(employee.ToDto());
    }
}
