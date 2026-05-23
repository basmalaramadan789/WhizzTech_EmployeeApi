using Xunit;
using Moq;
using FluentAssertions;
using WhizzTech.EmployeeApi.Application.Features.Employees.Commands.CreateEmployee;
using WhizzTech.EmployeeApi.Domain.Entities;
using WhizzTech.EmployeeApi.Domain.Exceptions;
using WhizzTech.EmployeeApi.Domain.Repositories;
using WhizzTech.EmployeeApi.Domain.ValueObjects;

namespace WhizzTech.EmployeeApi.UnitTests.Employees.Commands;

public class CreateEmployeeHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly CreateEmployeeCommandHandler _handler;

    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public CreateEmployeeHandlerTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _handler = new CreateEmployeeCommandHandler(_repositoryMock.Object);
    }

    private static CreateEmployeeCommand BuildCommand(
        string email = "john.doe@test.com",
        string firstName = "John",
        string lastName = "Doe",
        string department = "Engineering")
        => new(
            TenantId: TenantId,
            FirstName: firstName,
            LastName: lastName,
            Email: email,
            Department: department,
            CustomData: null,
            SalaryAmountMinor: 500000,
            SalaryCurrencyCode: "USD",
            CreatedBy: "test-user");

    [Fact]
    public async Task Handle_HappyPath_ShouldCreateEmployeeAndReturnDto()
    {
        // Arrange
        var command = BuildCommand();

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), TenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Data!.FirstName.Should().Be("John");
        result.Data.LastName.Should().Be("Doe");
        result.Data.Email.Should().Be("john.doe@test.com");
        result.Data.Department.Should().Be("Engineering");
        result.Data.Status.Should().Be("Active");
        result.Data.TenantId.Should().Be(TenantId);
        result.Data.Salary.AmountMinor.Should().Be(500000);
        result.Data.Salary.CurrencyCode.Should().Be("USD");
        result.Data.Salary.DisplayAmount.Should().Be(5000.00m);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldThrowDuplicateEmailException()
    {
        var command = BuildCommand(email: "duplicate@test.com");

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync("duplicate@test.com", TenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateEmailException>()
            .WithMessage("*duplicate@test.com*");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidEmail_Should_NotCallRepository()
    {
        var command = BuildCommand(email: "notavalidemail");

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), TenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        var result = await _handler.Handle(command, CancellationToken.None);
        result.Data.Should().NotBeNull();
    }
}
