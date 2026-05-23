namespace WhizzTech.EmployeeApi.Api.Requests;

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Department,
    Dictionary<string, object?>? CustomData,
    long? SalaryAmountMinor,
    string? SalaryCurrencyCode
);
