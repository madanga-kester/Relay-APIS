# Backend tests

The current test project includes database-independent unit coverage for:

- 25% platform fee and ledger reconciliation math
- qualified-click budget caps
- campaign lifecycle transitions
- request validation and campaign capacity rules

Database-backed integration tests should run against an isolated PostgreSQL database created from `docker-compose.dev.yml` after the first reviewed EF migration is generated. Those tests will cover cookie authentication, RBAC/ownership, idempotent clicks, transaction rollback, constraints, health checks, and critical workflows.
