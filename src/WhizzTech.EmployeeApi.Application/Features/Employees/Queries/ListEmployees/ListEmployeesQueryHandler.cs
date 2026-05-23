using MediatR;
using WhizzTech.EmployeeApi.Application.Common.DTOs;
using WhizzTech.EmployeeApi.Application.Common.Mappings;
using WhizzTech.EmployeeApi.Domain.Repositories;

namespace WhizzTech.EmployeeApi.Application.Features.Employees.Queries.ListEmployees;

public class ListEmployeesQueryHandler
    : IRequestHandler<ListEmployeesQuery, ApiResponse<IReadOnlyList<EmployeeDto>>>
{
    private readonly IEmployeeRepository _repository;

    public ListEmployeesQueryHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IReadOnlyList<EmployeeDto>>> Handle(
        ListEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

        var (items, totalCount) = await _repository.ListAsync(
            tenantId: request.TenantId,
            page: page,
            pageSize: pageSize,
            department: request.Department,
            status: request.Status,
            searchTerm: request.SearchTerm,
            ct: cancellationToken);

        var dtos = items.Select(e => e.ToDto()).ToList().AsReadOnly();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return ApiResponse<IReadOnlyList<EmployeeDto>>.Success(
            dtos,
            new PaginationDto(page, pageSize, totalCount, totalPages));
    }
}
