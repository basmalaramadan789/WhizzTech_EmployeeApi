using FluentValidation;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100).WithMessage("FirstName must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100).WithMessage("LastName must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100).WithMessage("Department must not exceed 100 characters.");

        When(x => x.SalaryAmountMinor.HasValue || x.SalaryCurrencyCode is not null, () =>
        {
            RuleFor(x => x.SalaryAmountMinor)
                .NotNull().WithMessage("SalaryAmountMinor is required when providing salary.")
                .GreaterThanOrEqualTo(0).WithMessage("SalaryAmountMinor must be non-negative.");

            RuleFor(x => x.SalaryCurrencyCode)
                .NotEmpty().WithMessage("SalaryCurrencyCode is required when providing salary.")
                .Length(3).WithMessage("SalaryCurrencyCode must be a 3-letter ISO 4217 code.")
                .Matches(@"^[A-Za-z]{3}$").WithMessage("SalaryCurrencyCode must contain only letters.");
        });
    }
}
