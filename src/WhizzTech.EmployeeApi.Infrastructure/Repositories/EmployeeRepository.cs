using Microsoft.EntityFrameworkCore;
using WhizzTech.EmployeeApi.Domain.Entities;
using WhizzTech.EmployeeApi.Domain.Enums;
using WhizzTech.EmployeeApi.Domain.Repositories;
using WhizzTech.EmployeeApi.Infrastructure.Persistence;

namespace WhizzTech.EmployeeApi.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {

        return await _context.Employees
            .Where(e => e.Id == id && e.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        Guid tenantId,
        Guid? excludeId = null,
        CancellationToken ct = default)
    {
        var query = _context.Employees
            .Where(e => e.Email == email.ToLowerInvariant() && e.TenantId == tenantId);

        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? department,
        string? status,
        string? searchTerm,
        CancellationToken ct = default)
    {
        var query = _context.Employees
            .Where(e => e.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(e => e.Department.ToLower() == department.ToLower());

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EmployeeStatus>(status, true, out var parsedStatus))
            query = query.Where(e => e.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Employee employee, CancellationToken ct = default)
    {
        await _context.Employees.AddAsync(employee, ct);
    }

    public Task UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        _context.Employees.Update(employee);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
