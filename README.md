# Clinic Queue

A small queue / check-in tool for a clinic front desk.

Patients check in at the desk (or on a tablet), get a ticket number and a short
code, and can look up their place in the queue from their phone. Front desk
staff work the queue from a dashboard, and a waiting room board shows which
tickets are being called.

## Features

- Patient check-in with a daily ticket number and lookup code
- Status page with place in queue and an estimated wait (updates itself)
- Staff dashboard: call / start / complete / no-show / requeue / cancel, plus notes
- Live queue panel on the dashboard that polls for new check-ins
- Urgent check-ins go to the front of the queue
- Waiting room board at `/Board` for a TV in the waiting area
- Cookie sign-in for staff

## Stack

- ASP.NET Core 5 (Razor Pages, server rendered)
- Entity Framework Core 5 + SQL Server
- A little vanilla JS for the polling bits, no front-end build step
- xUnit + EF Core InMemory for tests

## Project layout

```
src/ClinicQueue.Web/        the web app
  Data/                     DbContext and development seeding
  Migrations/               EF Core migrations
  Models/                   entities
  Pages/                    Razor Pages (Staff/ requires sign-in)
  Services/                 queue logic (QueueService), clock, helpers
  wwwroot/js/               queue-poll.js (dashboard), status-poll.js (patients)
tests/ClinicQueue.Tests/    unit tests
```

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

In Development the app applies migrations on startup and creates a staff
account from the `Seed` section of `appsettings.Development.json`.

## Configuration

| Setting | What it does |
| --- | --- |
| `ConnectionStrings:ClinicDb` | SQL Server connection string |
| `Clinic:Name` | Shown in the header and on the waiting room board |
| `Seed:StaffUsername` / `Seed:StaffPassword` | Development only: staff account created on startup |

Anything can be overridden with environment variables, e.g.
`Clinic__Name="Riverside Family Clinic"`.

## Resetting the development database

```
dotnet ef database drop --project src/ClinicQueue.Web
dotnet run --project src/ClinicQueue.Web
```

The app re-applies migrations and re-seeds the staff account on the next start.

## Tests

```
dotnet test
```
