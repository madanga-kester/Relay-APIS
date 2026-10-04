# Relay Backend

Independent ASP.NET Core 8 backend for the Relay advertising marketplace.

## Goals

- PostgreSQL with Entity Framework Core and Npgsql
- Secure HTTP-only cookie authentication with server-side RBAC
- Modular monolith: API, application services, domain, and infrastructure are separated
- Production-safe validation, error handling, health checks, rate limiting, audit history, and financial idempotency
- Cloud object storage through an S3-compatible abstraction
- Mailtrap SMTP for development email testing only
- No payment provider integration in this phase
- Linux-friendly deployment with environment-only secrets

The existing React/Vite frontend and TypeScript server are intentionally not modified by this backend foundation.

## Run locally

```bash
export PATH="$HOME/.dotnet:$PATH"
cd backend
dotnet restore
dotnet build
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Relay.Api
```

Set `ConnectionStrings__Postgres` to a PostgreSQL connection string before running the database-backed API. Development can use Mailtrap through `Mail__Host`, `Mail__Port`, `Mail__Username`, and `Mail__Password`.

## Project layout

- `src/Relay.Domain`: entities, enums, invariants, and financial value objects
- `src/Relay.Application`: use-case contracts, DTOs, validation, and application services
- `src/Relay.Infrastructure`: EF Core/PostgreSQL, email, cloud storage, persistence, and external adapters
- `src/Relay.Api`: HTTP pipeline, cookie auth, versioned controllers, middleware, health checks, and security headers
- `tests/Relay.Api.Tests`: unit and integration test foundation
- `docs`: deployment, security, data recovery, and API notes

## Important boundary

This backend is being built as a solution of its own. It is not yet wired into the React application. Frontend integration will be a later, explicit phase after the backend contracts and migrations have been reviewed.
