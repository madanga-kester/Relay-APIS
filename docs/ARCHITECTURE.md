# Backend architecture and operations

## Modular monolith

Relay uses one ASP.NET Core process with strict project boundaries instead of premature microservices. Controllers are transport-only. Application services own use cases and authorization decisions. Domain objects own marketplace invariants. Infrastructure owns EF Core, PostgreSQL, Mailtrap/SMTP, and S3-compatible cloud storage.

## Environments

- Development: local configuration plus Mailtrap SMTP; verbose developer diagnostics only locally.
- Staging: production-like PostgreSQL, cloud storage, HTTPS, non-production email credentials, safe migration rehearsal.
- Production: environment/secret-manager configuration only, HTTPS termination, secure cookies, structured logs, no Swagger or detailed exceptions unless explicitly enabled.

No secret belongs in source control. Use `ConnectionStrings__Postgres`, `Auth__DataProtectionKeysPath`, `Mail__*`, and `Storage__*` environment variables or a secret manager.

## Authentication

Cookie authentication is server-side and HTTP-only. Cookies are Secure in non-development, SameSite Lax by default, and antiforgery is required for unsafe browser requests. Authorization policies use the persisted role claim and every resource query also checks ownership. Admin access is a separate policy, not a frontend convention.

## Data integrity

Qualified clicks are idempotent by click ID and request idempotency key. A qualified click, ledger entries, campaign budget status, and activity record are written in one transaction. PostgreSQL unique constraints protect duplicate tracking and ledger operations. Application-level authorization remains mandatory even if PostgreSQL RLS is enabled in the production migration.

## Background work

The first implementation keeps ordinary CRUD and click qualification synchronous. Email delivery and reconciliation are isolated behind interfaces so a worker/queue can be added when volume justifies it. No Redis, Kubernetes, or message broker is introduced without measured need.

## Deployment and recovery

Run database migrations as a separate controlled deployment step, never automatically on every application startup in production. Backups must be automated by the PostgreSQL host, retained according to policy, and restored regularly in staging. Rollback means redeploying the previous immutable application image and applying a backward-compatible database migration strategy; destructive migrations require a separate release.
