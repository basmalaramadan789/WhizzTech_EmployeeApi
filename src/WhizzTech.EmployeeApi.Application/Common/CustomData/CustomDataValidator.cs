using System.Text.Json;
using WhizzTech.EmployeeApi.Domain.Exceptions;

namespace WhizzTech.EmployeeApi.Application.Common.CustomData;

public static class CustomDataValidator
{
    public static void Validate(Guid tenantId, Dictionary<string, object?> customData)
    {
        var definitions = TenantCustomFieldRegistry.GetDefinitions(tenantId);
        if (definitions.Count == 0) return;

        foreach (var def in definitions)
        {
            if (!customData.TryGetValue(def.FieldName, out var value))
            {
                if (def.IsRequired)
                    throw new CustomDataValidationException(
                        $"Custom field '{def.FieldName}' is required for this tenant.");
                continue;
            }

            if (value is null) continue;

            bool isValid;

            if (value is JsonElement jsonElement)
            {
                isValid = def.FieldType switch
                {
                    "string"  => jsonElement.ValueKind == JsonValueKind.String,
                    "number"  => jsonElement.ValueKind == JsonValueKind.Number,
                    "boolean" => jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False,
                    _         => true
                };
            }
            else
            {
                isValid = def.FieldType switch
                {
                    "string"  => value is string,
                    "number"  => value is int or long or double or float or decimal,
                    "boolean" => value is bool,
                    _         => true
                };
            }

            if (!isValid)
                throw new CustomDataValidationException(
                    $"Custom field '{def.FieldName}' must be of type '{def.FieldType}'.");
        }
    }
}
