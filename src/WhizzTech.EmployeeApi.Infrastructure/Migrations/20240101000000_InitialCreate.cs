using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhizzTech.EmployeeApi.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tasks", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CustomData = table.Column<string>(type: "jsonb", nullable: false),
                SalaryAmountMinor = table.Column<long>(type: "bigint", nullable: false),
                SalaryCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employees", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Employees_TenantId",
            table: "Employees",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_TenantId_Email_Unique",
            table: "Employees",
            columns: new[] { "TenantId", "Email" },
            unique: true,
            filter: "\"DeletedAt\" IS NULL");

        migrationBuilder.Sql(@"
            ALTER TABLE ""Employees"" ENABLE ROW LEVEL SECURITY;

            CREATE POLICY tenant_isolation_policy ON ""Employees""
                USING (""TenantId"" = current_setting('app.current_tenant_id', true)::uuid)
                WITH CHECK (""TenantId"" = current_setting('app.current_tenant_id', true)::uuid);
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP POLICY IF EXISTS tenant_isolation_policy ON ""Employees"";
            ALTER TABLE ""Employees"" DISABLE ROW LEVEL SECURITY;
        ");

        migrationBuilder.DropTable(name: "Employees");
        migrationBuilder.DropTable(name: "Tasks");
    }
}
