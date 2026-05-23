using WhizzTech.EmployeeApi.Domain.Entities;

namespace WhizzTech.EmployeeApi.Domain.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, Guid tenantId, Guid? excludeId = null, CancellationToken ct = default);
    Task<(IReadOnlyList<Employee> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? department,
        string? status,
        string? searchTerm,
        CancellationToken ct = default);
    Task AddAsync(Employee employee, CancellationToken ct = default);
    Task UpdateAsync(Employee employee, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
