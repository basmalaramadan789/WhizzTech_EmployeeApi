using Xunit;
using Moq;
using FluentAssertions;
using WhizzTech.EmployeeApi.Application.Features.Employees.Commands.CreateEmployee;
using WhizzTech.EmployeeApi.Application.Features.Employees.Queries.ListEmployees;
using WhizzTech.EmployeeApi.Domain.Entities;
using WhizzTech.EmployeeApi.Domain.Repositories;
using WhizzTech.EmployeeApi.Domain.ValueObjects;

namespace WhizzTech.EmployeeApi.UnitTests.Employees;


public class TenantIsolationTests
{
    private static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task ListEmployees_TenantA_ShouldNotReturnTenantBData()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();
        var handler = new ListEmployeesQueryHandler(repositoryMock.Object);

        var tenantAEmployee = Employee.Create(TenantAId, "Alice", "Smith", "alice@a.com", "HR", null,
            new Money(600000, "USD"), "test");

        repositoryMock
            .Setup(r => r.ListAsync(TenantAId, 1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Employee>)new[] { tenantAEmployee }, 1));

        var query = new ListEmployeesQuery(TenantAId, 1, 10, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Data.Should().AllSatisfy(e => e.TenantId.Should().Be(TenantAId));
        result.Data.Should().NotContain(e => e.TenantId == TenantBId);

        repositoryMock.Verify(
            r => r.ListAsync(TenantAId, It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateEmployee_TenantA_ShouldIsolateEmailCheckWithinTenant()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();
        var handler = new CreateEmployeeCommandHandler(repositoryMock.Object);

        repositoryMock
            .Setup(r => r.ExistsByEmailAsync("duplicate@test.com", TenantAId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repositoryMock
            .Setup(r => r.ExistsByEmailAsync("duplicate@test.com", TenantBId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var tenantACommand = new CreateEmployeeCommand(
            TenantId: TenantAId,
            FirstName: "Bob",
            LastName: "Jones",
            Email: "duplicate@test.com",
            Department: "Sales",
            CustomData: null,
            SalaryAmountMinor: 500000,
            SalaryCurrencyCode: "USD",
            CreatedBy: "test-user");

        var tenantBCommand = tenantACommand with { TenantId = TenantBId };

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var tenantAAction = async () => await handler.Handle(tenantACommand, CancellationToken.None);
        await tenantAAction.Should().ThrowAsync<Domain.Exceptions.DuplicateEmailException>();

        var tenantBResult = await handler.Handle(tenantBCommand, CancellationToken.None);
        tenantBResult.Data.Should().NotBeNull();
        tenantBResult.Data!.TenantId.Should().Be(TenantBId);
    }

    [Fact]
    public async Task GetEmployeeById_WrongTenant_ShouldReturnNotFound()
    {
       
        var repositoryMock = new Mock<IEmployeeRepository>();
        var handler = new Application.Features.Employees.Queries.GetEmployeeById.GetEmployeeByIdQueryHandler(
            repositoryMock.Object);

        var employeeId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, TenantBId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var query = new Application.Features.Employees.Queries.GetEmployeeById.GetEmployeeByIdQuery(
            Id: employeeId,
            TenantId: TenantBId);

        var act = async () => await handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<Domain.Exceptions.EmployeeNotFoundException>();
    }
}
