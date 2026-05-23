namespace WhizzTech.EmployeeApi.Application.Common.CustomData;


public record CustomFieldDefinition(
    string FieldName,
    string FieldType,      
    bool IsRequired
);


public static class TenantCustomFieldRegistry
{
    private static readonly Dictionary<Guid, List<CustomFieldDefinition>> _definitions = new();

    public static void Register(Guid tenantId, List<CustomFieldDefinition> fields)
        => _definitions[tenantId] = fields;

    public static List<CustomFieldDefinition> GetDefinitions(Guid tenantId)
        => _definitions.TryGetValue(tenantId, out var defs) ? defs : new();

    public static IReadOnlyDictionary<Guid, List<CustomFieldDefinition>> All => _definitions;
}
