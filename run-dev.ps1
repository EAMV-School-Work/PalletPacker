$ErrorActionPreference = "Stop"

# Find project root
Set-Location $PSScriptRoot

# Read database password
$envFile = Join-Path $PSScriptRoot ".env"

if (-not (Test-Path $envFile)) {
    throw "Missing .env file. Copy .env.example to .env first."
}

$passwordLine = Get-Content $envFile |
    Where-Object { $_ -match '^MSSQL_SA_PASSWORD=' } |
    Select-Object -First 1

if (-not $passwordLine) {
    throw "MSSQL_SA_PASSWORD is missing from .env."
}

$password = $passwordLine -replace '^MSSQL_SA_PASSWORD=', ''

if ([string]::IsNullOrWhiteSpace($password)) {
    throw "MSSQL_SA_PASSWORD cannot be empty."
}

# Configure EF-Core connection
$connectionString = New-Object System.Data.Common.DbConnectionStringBuilder
$connectionString["Server"] = "tcp:127.0.0.1,14330"
$connectionString["Database"] = "PalletPacker"
$connectionString["User ID"] = "sa"
$connectionString["Password"] = $password
$connectionString["Encrypt"] = "True"
$connectionString["TrustServerCertificate"] = "True"

$env:ConnectionStrings__DefaultConnection = $connectionString.ConnectionString

# Make sure MS SQL Server is running
docker compose up -d database

if ($LASTEXITCODE -ne 0) {
    throw "Failed to start SQL Server."
}

# Start ASP.NET locally
dotnet run --project src/PalletPacker.Web
