FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY WhizzTech.EmployeeApi.slnx ./
COPY src/WhizzTech.EmployeeApi.Domain/WhizzTech.EmployeeApi.Domain.csproj src/WhizzTech.EmployeeApi.Domain/
COPY src/WhizzTech.EmployeeApi.Application/WhizzTech.EmployeeApi.Application.csproj src/WhizzTech.EmployeeApi.Application/
COPY src/WhizzTech.EmployeeApi.Infrastructure/WhizzTech.EmployeeApi.Infrastructure.csproj src/WhizzTech.EmployeeApi.Infrastructure/
COPY src/WhizzTech.EmployeeApi.Api/WhizzTech.EmployeeApi.Api.csproj src/WhizzTech.EmployeeApi.Api/

# Restore NuGet packages
RUN dotnet restore src/WhizzTech.EmployeeApi.Api/WhizzTech.EmployeeApi.Api.csproj

# Copy all source files
COPY src/ src/

# Build & publish
RUN dotnet publish src/WhizzTech.EmployeeApi.Api/WhizzTech.EmployeeApi.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Create non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "WhizzTech.EmployeeApi.Api.dll"]
