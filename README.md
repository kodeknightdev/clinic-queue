# Clinic Queue

A small queue / check-in tool for a clinic front desk.

Patients check in at the desk (or on a tablet), get a ticket number, and can
look up where they are in the queue. Front desk staff work the queue from a
dashboard.

## Stack

- ASP.NET Core 5 (Razor Pages)
- Entity Framework Core 5 + SQL Server

## Running locally

Requires the .NET 5 SDK and SQL Server (LocalDB is fine on Windows).

```
dotnet tool restore
dotnet ef database update --project src/ClinicQueue.Web
dotnet run --project src/ClinicQueue.Web
```

The connection string lives in `appsettings.json` under `ConnectionStrings:ClinicDb`.
On macOS/Linux point it at a SQL Server container instead of LocalDB, e.g.

```
docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=<your password> -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest
```

## Tests

```
dotnet test
```
