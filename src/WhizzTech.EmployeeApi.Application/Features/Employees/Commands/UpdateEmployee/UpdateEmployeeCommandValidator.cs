using FluentValidation;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Employee Id is required.");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(255);

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be Active or Suspended.");

        When(x => x.SalaryAmountMinor.HasValue || x.SalaryCurrencyCode is not null, () =>
        {
            RuleFor(x => x.SalaryAmountMinor)
                .NotNull()
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.SalaryCurrencyCode)
                .NotEmpty()
                .Length(3)
                .Matches(@"^[A-Za-z]{3}$");
        });
    }
}
