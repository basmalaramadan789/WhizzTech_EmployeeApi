using FluentAssertions;
using FluentValidation;
using Xunit;
using WhizzTech.EmployeeApi.Application.Features.Employees.Commands.CreateEmployee;

namespace WhizzTech.EmployeeApi.UnitTests.Employees.Validators;

public class CreateEmployeeValidatorTests
{
    private readonly CreateEmployeeCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    public async Task Validate_InvalidEmail_ShouldFail(string email)
    {
        var command = new CreateEmployeeCommand(
            TenantId: Guid.NewGuid(),
            FirstName: "John",
            LastName: "Doe",
            Email: email,
            Department: "HR",
            CustomData: null,
            SalaryAmountMinor: null,
            SalaryCurrencyCode: null,
            CreatedBy: "test");

        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_EmptyFirstName_ShouldFail()
    {
        var command = new CreateEmployeeCommand(
            TenantId: Guid.NewGuid(),
            FirstName: "",
            LastName: "Doe",
            Email: "valid@email.com",
            Department: "HR",
            CustomData: null,
            SalaryAmountMinor: null,
            SalaryCurrencyCode: null,
            CreatedBy: "test");

        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateEmployeeCommand(
            TenantId: Guid.NewGuid(),
            FirstName: "Alice",
            LastName: "Smith",
            Email: "alice.smith@company.com",
            Department: "Engineering",
            CustomData: null,
            SalaryAmountMinor: 800000,
            SalaryCurrencyCode: "USD",
            CreatedBy: "admin");

        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("123")]
    public async Task Validate_InvalidCurrencyCode_ShouldFail(string currencyCode)
    {
        var command = new CreateEmployeeCommand(
            TenantId: Guid.NewGuid(),
            FirstName: "John",
            LastName: "Doe",
            Email: "john@test.com",
            Department: "HR",
            CustomData: null,
            SalaryAmountMinor: 500000,
            SalaryCurrencyCode: currencyCode,
            CreatedBy: "test");

        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }
}
