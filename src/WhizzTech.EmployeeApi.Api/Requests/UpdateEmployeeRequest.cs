namespace WhizzTech.EmployeeApi.Api.Requests;

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
