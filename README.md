# WhizzTech Multi-Tenant Employee API

This is a clean, practical, and highly robust multi-tenant Employee REST API built for the WhizzTech technical assessment. The application is built on **.NET 8**, **PostgreSQL**, **EF Core**, and **Docker**, utilizing modern software design patterns like Clean Architecture and CQRS.

---

## Technical Design & Architectural Choices

When building this API, I prioritized **security (isolation)**, **type safety**, and **testability**. Here is a breakdown of the core design decisions made throughout the project:

### 1. Clean Architecture (Layered Separation)
The codebase is divided into four distinct projects to enforce a strict separation of concerns and prevent leakage of database or framework details into our business logic:
* **Domain (`WhizzTech.EmployeeApi.Domain`):** The absolute core. Contains our entities (`Employee`, `TaskItem`), value objects (`Money`), custom exceptions, and repository interfaces. It has zero external dependencies.
* **Application (`WhizzTech.EmployeeApi.Application`):** Contains our business use cases (CQRS Commands and Queries), FluentValidation validators, DTO mappings, and MediatR pipelines.
* **Infrastructure (`WhizzTech.EmployeeApi.Infrastructure`):** Implements the database context (`AppDbContext`), EF Core configurations, database migrations, and repository implementations.
* **API (`WhizzTech.EmployeeApi.Api`):** The entry point. Houses our controllers, custom HTTP middleware (e.g., exception handling), and Docker configuration.

### 2. Multi-Tenant Isolation Strategy (Defense-in-Depth)
Cross-tenant data leakage is one of the most critical security issues in multi-tenant systems. To completely prevent this, I implemented isolation at multiple independent layers:
* **API Level (Header Parsing):** Every request must supply a valid `X-Tenant-Id` header (e.g., in our controller endpoints). If it's missing or not a valid UUID, the request is immediately rejected.
* **Repository Level (Explicit Scoping):** The `EmployeeRepository` enforces a strict `.Where(e => e.TenantId == tenantId)` check on every read, write, and duplicate check query.
* **EF Core Global Query Filters:** Soft-deleted records are automatically filtered out globally (`DeletedAt == null`) so developers don't have to manually append this check to every LINQ query.
* **PostgreSQL Row-Level Security (RLS) - *Bonus Implemented*:** As an ultimate safety guard, I added a PostgreSQL Row-Level Security policy inside the migration. This forces PostgreSQL itself to reject queries if they don't match the current tenant ID session variable, preventing accidental exposure even if a developer makes a bug in the C# query!
* **Tenant-Scoped Unique Constraints:** Employees can have the same email address across *different* tenants, but the email must be unique *within* the same tenant. We achieved this with a tenant-scoped unique index filter: `"TenantId", "Email" WHERE "DeletedAt" IS NULL`.

### 3. CQRS with MediatR & Automatic Validation
We separate read operations (Queries) from write operations (Commands). This keeps our handlers small, single-purpose, and incredibly easy to unit test.
* Validation is handled automatically before requests reach our handlers using a **MediatR Pipeline Behavior (`ValidationPipelineBehavior`)**.
* If any FluentValidation rule fails, the pipeline intercepts it and throws a custom exception, which our global exception handler returns as a neat validation error response.

### 4. Salary & The Money Value Object
To represent employee salaries, I avoided `FLOAT` or `DECIMAL` due to known binary rounding issues and instead implemented a DDD **`Money` Value Object**:
* It stores the currency amount in its smallest minor unit (e.g., `850000` instead of `8500.00`) using `BIGINT`.
* It pairs the amount with an ISO 4217 3-letter currency code (e.g., `USD`, `EGP`).
* This approach ensures perfect precision across all mathematical operations and database storage.

### 5. Flexible Tenant Custom Data (JSONB)
To handle tenant-specific employee fields, the table uses a PostgreSQL `JSONB` column named `CustomData`.
* I built an extensible registry system (`TenantCustomFieldRegistry`) that maps required/optional fields and types per tenant.
* **Tenant A** requires `badge_color` (string) and `clearance_level` (number).
* **Tenant B** requires `office_location` (string) and `remote` (boolean).
* A custom validation service (`CustomDataValidator`) validates these types and required fields dynamically on both Create and Update requests, handling JSON deserialization safely.

### 6. PascalCase Database Convention
As requested, all tables and columns are explicitly configured to use PascalCase naming (e.g., `Employees`, `Tasks`, `FirstName`, `DeletedAt`) inside our EF Core entity configurations.

---

## Project Features

* **.NET 8 Web API** with Swagger UI.
* **PostgreSQL Integration** with fully configured Migrations and Seeding.
* **Custom Tasks Table** with fields `Id`, `Name`, `Description`, and `DueDate`.
* **Soft Deletes** (`DeletedAt` timestamp is recorded instead of permanent row deletion).
* **Search, Filters, and Pagination** on the employee listing endpoint.
* **Response Envelope** pattern: Every single API response follows the `{"data": ..., "pagination": ..., "error": ...}` standard structure.
* **Audit Fields** (`CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` tracking).
* **Docker Setup** that spins up both the database and the API with a single run command.
* **High Test Coverage** including both Unit Tests and real Database Integration Tests.

---

## Quick Start (Docker)

To run the entire ecosystem (PostgreSQL + API + Swagger) in one command:

```bash
# Clone and navigate to the directory
cd WhizzTech.EmployeeApi

# Build and start the containers
docker compose up --build
```

* **Swagger UI:** `http://localhost:8080/swagger`
* **Postgres:** `localhost:5432`

The database migrations run automatically on startup, and seed data is instantly inserted for both tenants.

---

## Running Locally for Development

If you prefer running the API locally in your IDE or via the command line:

1. **Start the database container:**
   ```bash
   docker compose up postgres -d
   ```
2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```
3. **Run the API project:**
   ```bash
   cd src/WhizzTech.EmployeeApi.Api
   dotnet run
   ```
4. **Open in browser:** `https://localhost:7299/swagger` (or `http://localhost:5269/swagger`)

---

## Testing Strategy

I wrote two distinct test suites to verify that the application is fully functional and secure:

### 1. Unit Tests
These focus on pure business logic, checking our CQRS handlers and validation pipeline in memory.
```bash
dotnet test tests/WhizzTech.EmployeeApi.UnitTests
```
* **What's tested:** `CreateEmployeeCommandHandler` (happy path + duplicate email checks), `ListEmployeesQueryHandler` (pagination offsets, page-limit capping), and FluentValidation rules (email formatting, invalid money currencies).

### 2. Integration Tests
These run end-to-end against a real PostgreSQL instance to ensure our database schema, unique indexes, custom types, and tenant isolation policies are 100% correct in a production-like environment.
```bash
dotnet test tests/WhizzTech.EmployeeApi.IntegrationTests
```
* **What's tested:** Database write/read logic, validating that Tenant A can never retrieve Tenant B's data, duplicate email handling across multiple tenants, soft delete verification (fetching a soft-deleted record returns `404`), and missing multi-tenant headers.

---

## API Reference

### Required Headers
* `X-Tenant-Id` (UUID) — The ID of the tenant you are accessing.
* `X-User-Id` (String, Optional) — The identity of the caller for auditing. Falls back to `anonymous` if omitted.

### Seeded Tenants to Test With:
* **Tenant A ID:** `11111111-1111-1111-1111-111111111111`
  * *Required Custom Fields:* `badge_color` (string), `clearance_level` (number)
* **Tenant B ID:** `22222222-2222-2222-2222-222222222222`
  * *Required Custom Fields:* `office_location` (string), `remote` (boolean)

### Endpoints
* **`POST /api/v1/employees`** — Create an employee.
* **`GET /api/v1/employees`** — List employees (supports search query, department, status, and pagination params `page` & `pageSize`).
* **`GET /api/v1/employees/{id}`** — Retrieve a single employee's details.
* **`PUT /api/v1/employees/{id}`** — Update an existing employee.
* **`DELETE /api/v1/employees/{id}`** — Soft-delete an employee.
