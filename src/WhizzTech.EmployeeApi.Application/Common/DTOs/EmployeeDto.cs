using WhizzTech.EmployeeApi.Domain.Enums;

namespace WhizzTech.EmployeeApi.Application.Common.DTOs;

public record EmployeeDto(
    Guid Id,
    Guid TenantId,
    string FirstName,
    string LastName,
    string Email,
    string Department,
    string Status,
    Dictionary<string, object?> CustomData,
    SalaryDto Salary,
    string CreatedBy,
    string UpdatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record SalaryDto(long AmountMinor, string CurrencyCode, decimal DisplayAmount);

public record PaginationDto(int Page, int PageSize, int TotalCount, int TotalPages);

public record ApiResponse<T>
{
    public T? Data { get; init; }
    public PaginationDto? Pagination { get; init; }
    public string? Error { get; init; }

    public static ApiResponse<T> Success(T data, PaginationDto? pagination = null)
        => new() { Data = data, Pagination = pagination };

    public static ApiResponse<T> Failure(string error)
        => new() { Error = error };
}
