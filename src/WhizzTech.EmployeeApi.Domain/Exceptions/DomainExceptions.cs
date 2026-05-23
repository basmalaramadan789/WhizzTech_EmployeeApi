namespace WhizzTech.EmployeeApi.Domain.Exceptions;

public class EmployeeNotFoundException : Exception
{
    public EmployeeNotFoundException(Guid id)
        : base($"Employee with ID '{id}' was not found.") { }
}

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"An employee with email '{email}' already exists in this tenant.") { }
}

public class InvalidTenantException : Exception
{
    public InvalidTenantException()
        : base("A valid X-Tenant-Id header is required.") { }
}

public class CustomDataValidationException : Exception
{
    public CustomDataValidationException(string message) : base(message) { }
}
