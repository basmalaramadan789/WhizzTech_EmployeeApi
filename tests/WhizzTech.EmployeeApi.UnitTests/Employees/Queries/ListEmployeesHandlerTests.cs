using Xunit;
using Moq;
using FluentAssertions;
using WhizzTech.EmployeeApi.Application.Features.Employees.Queries.ListEmployees;
using WhizzTech.EmployeeApi.Domain.Entities;
using WhizzTech.EmployeeApi.Domain.Repositories;
using WhizzTech.EmployeeApi.Domain.ValueObjects;

namespace WhizzTech.EmployeeApi.UnitTests.Employees.Queries;

public class ListEmployeesHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly ListEmployeesQueryHandler _handler;

    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public ListEmployeesHandlerTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _handler = new ListEmployeesQueryHandler(_repositoryMock.Object);
    }

    private static List<Employee> BuildEmployees(int count, Guid tenantId)
    {
        var employees = new List<Employee>();
        for (int i = 0; i < count; i++)
        {
            employees.Add(Employee.Create(
                tenantId,
                $"First{i}",
                $"Last{i}",
                $"employee{i}@tenant.com",
                "Engineering",
                null,
                new Money(500000, "USD"),
                "test"));
        }
        return employees;
    }

    [Fact]
    public async Task Handle_FirstPage_ShouldReturnCorrectPaginationMetadata()
    {
        // Arrange
        var employees = BuildEmployees(25, TenantId);
        var firstPageEmployees = employees.Take(10).ToList();

        _repositoryMock
            .Setup(r => r.ListAsync(TenantId, 1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Employee>)firstPageEmployees, 25));

        var query = new ListEmployeesQuery(TenantId, 1, 10, null, null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.Pagination.Should().NotBeNull();
        result.Pagination!.Page.Should().Be(1);
        result.Pagination.PageSize.Should().Be(10);
        result.Pagination.TotalCount.Should().Be(25);
        result.Pagination.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_LastPage_ShouldReturnRemainingItems()
    {
        var employees = BuildEmployees(5, TenantId);

        _repositoryMock
            .Setup(r => r.ListAsync(TenantId, 3, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Employee>)employees, 25));

        var query = new ListEmployeesQuery(TenantId, 3, 10, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().HaveCount(5);
        result.Pagination!.TotalPages.Should().Be(3);
        result.Pagination.Page.Should().Be(3);
    }

    [Fact]
    public async Task Handle_PageSizeCappedAt100_ShouldUseMaximum()
    {
        _repositoryMock
            .Setup(r => r.ListAsync(TenantId, 1, 100, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Employee>)new List<Employee>(), 0));

        var query = new ListEmployeesQuery(TenantId, 1, 999, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Pagination!.PageSize.Should().Be(100);
        _repositoryMock.Verify(
            r => r.ListAsync(TenantId, 1, 100, null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyListWithZeroPagination()
    {
        _repositoryMock
            .Setup(r => r.ListAsync(TenantId, 1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Employee>)new List<Employee>(), 0));

        var query = new ListEmployeesQuery(TenantId, 1, 10, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.Pagination!.TotalCount.Should().Be(0);
        result.Pagination.TotalPages.Should().Be(0);
    }
}
