using MediatR;
using Microsoft.AspNetCore.Mvc;
using WhizzTech.EmployeeApi.Application.Common.DTOs;
using WhizzTech.EmployeeApi.Application.Features.Employees.Commands.CreateEmployee;
using WhizzTech.EmployeeApi.Application.Features.Employees.Commands.DeleteEmployee;
using WhizzTech.EmployeeApi.Application.Features.Employees.Commands.UpdateEmployee;
using WhizzTech.EmployeeApi.Application.Features.Employees.Queries.GetEmployeeById;
using WhizzTech.EmployeeApi.Application.Features.Employees.Queries.ListEmployees;
using WhizzTech.EmployeeApi.Domain.Enums;

namespace WhizzTech.EmployeeApi.Api.Controllers;

[ApiController]
[Route("api/v1/employees")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetTenantId()
    {
        if (!Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader)
            || !Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            throw new Domain.Exceptions.InvalidTenantException();
        }
        return tenantId;
    }

    private string GetCallerIdentity()
        => Request.Headers.TryGetValue("X-User-Id", out var userId)
            ? userId.ToString()
            : "anonymous";

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromBody] CreateEmployeeRequest request,
        CancellationToken ct)
    {
        var command = new CreateEmployeeCommand(
            TenantId: tenantId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: request.Email,
            Department: request.Department,
            CustomData: request.CustomData,
            SalaryAmountMinor: request.SalaryAmountMinor,
            SalaryCurrencyCode: request.SalaryCurrencyCode,
            CreatedBy: GetCallerIdentity());

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmployeeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? department = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = new ListEmployeesQuery(tenantId, page, pageSize, department, status, search);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id, 
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken ct)
    {
        var query = new GetEmployeeByIdQuery(id, tenantId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken ct)
    {

        if (!Enum.TryParse<EmployeeStatus>(request.Status, true, out var status))
            return BadRequest(ApiResponse<EmployeeDto>.Failure($"Invalid status value: {request.Status}"));

        var command = new UpdateEmployeeCommand(
            Id: id,
            TenantId: tenantId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: request.Email,
            Department: request.Department,
            Status: status,
            CustomData: request.CustomData,
            SalaryAmountMinor: request.SalaryAmountMinor,
            SalaryCurrencyCode: request.SalaryCurrencyCode,
            UpdatedBy: GetCallerIdentity());

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id, 
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken ct)
    {
        var command = new DeleteEmployeeCommand(id, tenantId, GetCallerIdentity());
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Department,
    Dictionary<string, object?>? CustomData,
    long? SalaryAmountMinor,
    string? SalaryCurrencyCode
);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Department,
    string Status,
    Dictionary<string, object?>? CustomData,
    long? SalaryAmountMinor,
    string? SalaryCurrencyCode
);
