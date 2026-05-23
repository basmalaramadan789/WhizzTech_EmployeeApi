using Xunit;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WhizzTech.EmployeeApi.Application.Common.DTOs;
using WhizzTech.EmployeeApi.Infrastructure.Persistence;

namespace WhizzTech.EmployeeApi.IntegrationTests.Employees;

/// <summary>
/// Integration tests using a real PostgreSQL database via Testcontainers.
/// Tests tenant isolation end-to-end through the full HTTP stack.
/// </summary>
public class EmployeeIntegrationTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _clientA;
    private readonly HttpClient _clientB;

    private static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public EmployeeIntegrationTests(IntegrationTestFactory factory)
    {
        _factory = factory;

        _clientA = factory.CreateClient();
        _clientA.DefaultRequestHeaders.Add("X-Tenant-Id", TenantAId.ToString());
        _clientA.DefaultRequestHeaders.Add("X-User-Id", "test-user-a");

        _clientB = factory.CreateClient();
        _clientB.DefaultRequestHeaders.Add("X-Tenant-Id", TenantBId.ToString());
        _clientB.DefaultRequestHeaders.Add("X-User-Id", "test-user-b");
    }

    [Fact]
    public async Task CreateEmployee_TenantA_ShouldPersistToDatabase()
    {
        // Arrange
        var request = new
        {
            FirstName = "Integration",
            LastName = "TestUser",
            Email = $"integration.{Guid.NewGuid()}@tenanta.com",
            Department = "Engineering",
            CustomData = new Dictionary<string, object>
            {
                ["badge_color"] = "blue",
                ["clearance_level"] = 2
            },
            SalaryAmountMinor = 750000L,
            SalaryCurrencyCode = "USD"
        };

        // Act
        var response = await _clientA.PostAsJsonAsync("/api/v1/employees", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be("Integration");
        result.Data.TenantId.Should().Be(TenantAId);
        result.Data.Salary.AmountMinor.Should().Be(750000);
        result.Data.Salary.CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public async Task TenantIsolation_TenantACannotSeeTenantBEmployees()
    {
        // Arrange – create an employee under TenantB
        var email = $"tenantb.{Guid.NewGuid()}@company.com";
        var createRequest = new
        {
            FirstName = "TenantB",
            LastName = "Employee",
            Email = email,
            Department = "Marketing",
            CustomData = new Dictionary<string, object>
            {
                ["office_location"] = "NYC",
                ["remote"] = false
            },
            SalaryAmountMinor = 600000L,
            SalaryCurrencyCode = "USD"
        };

        var createResponse = await _clientB.PostAsJsonAsync("/api/v1/employees", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
        var employeeId = created!.Data!.Id;

        // Act – try to get TenantB employee using TenantA credentials
        var getResponse = await _clientA.GetAsync($"/api/v1/employees/{employeeId}");

        // Assert – TenantA cannot see TenantB's employee
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DuplicateEmail_SameTeant_ShouldReturn409()
    {
        // Arrange – create employee first
        var email = $"duplicate.{Guid.NewGuid()}@tenanta.com";
        var createRequest = new
        {
            FirstName = "First",
            LastName = "User",
            Email = email,
            Department = "HR",
            CustomData = new Dictionary<string, object>
            {
                ["badge_color"] = "green",
                ["clearance_level"] = 1
            },
            SalaryAmountMinor = 500000L,
            SalaryCurrencyCode = "USD"
        };

        var firstResponse = await _clientA.PostAsJsonAsync("/api/v1/employees", createRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act – create another employee with same email in same tenant
        var secondResponse = await _clientA.PostAsJsonAsync("/api/v1/employees", createRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await secondResponse.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
        error!.Error.Should().Contain(email);
    }

    [Fact]
    public async Task SameEmail_DifferentTenants_ShouldBothSucceed()
    {
        // Same email across different tenants must be allowed
        var email = $"shared.{Guid.NewGuid()}@company.com";
        var createRequest = new
        {
            FirstName = "Shared",
            LastName = "Email",
            Email = email,
            Department = "Finance",
            SalaryAmountMinor = 400000L,
            SalaryCurrencyCode = "USD"
        };

        // Tenant A request (no custom data needed if registry is empty for this path)
        var requestA = new
        {
            FirstName = "Shared",
            LastName = "Email",
            Email = email,
            Department = "Finance",
            CustomData = new Dictionary<string, object>
            {
                ["badge_color"] = "blue",
                ["clearance_level"] = 1
            },
            SalaryAmountMinor = 400000L,
            SalaryCurrencyCode = "USD"
        };
        var requestB = new
        {
            FirstName = "Shared",
            LastName = "Email",
            Email = email,
            Department = "Finance",
            CustomData = new Dictionary<string, object>
            {
                ["office_location"] = "NYC",
                ["remote"] = false
            },
            SalaryAmountMinor = 400000L,
            SalaryCurrencyCode = "USD"
        };

        var responseA = await _clientA.PostAsJsonAsync("/api/v1/employees", requestA);
        var responseB = await _clientB.PostAsJsonAsync("/api/v1/employees", requestB);

        responseA.StatusCode.Should().Be(HttpStatusCode.Created);
        responseB.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ListEmployees_WithPagination_ShouldReturnCorrectPage()
    {
        // Act
        var response = await _clientA.GetAsync("/api/v1/employees?page=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<EmployeeDto>>>();
        result.Should().NotBeNull();
        result!.Pagination.Should().NotBeNull();
        result.Pagination!.PageSize.Should().Be(2);
        result.Pagination.Page.Should().Be(1);
    }

    [Fact]
    public async Task SoftDelete_ThenGet_ShouldReturn404()
    {
        // Arrange – create employee
        var email = $"todelete.{Guid.NewGuid()}@tenanta.com";
        var createRequest = new
        {
            FirstName = "To",
            LastName = "Delete",
            Email = email,
            Department = "Temp",
            CustomData = new Dictionary<string, object>
            {
                ["badge_color"] = "red",
                ["clearance_level"] = 0
            },
            SalaryAmountMinor = 100000L,
            SalaryCurrencyCode = "USD"
        };

        var createResponse = await _clientA.PostAsJsonAsync("/api/v1/employees", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
        var id = created!.Data!.Id;

        // Act – soft delete
        var deleteResponse = await _clientA.DeleteAsync($"/api/v1/employees/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert – cannot retrieve deleted employee
        var getResponse = await _clientA.GetAsync($"/api/v1/employees/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MissingTenantHeader_ShouldReturn400()
    {
        var clientNoHeader = _factory.CreateClient();
        var response = await clientNoHeader.GetAsync("/api/v1/employees");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
