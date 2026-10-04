# Implementation status

## Completed in the foundation phase

- Independent `Relay.sln` modular monolith scaffold
- PostgreSQL/Npgsql dependency and development compose file
- Domain entities for users, campaigns, communities, applications, placements, clicks, ledger entries, activity, and reset tokens
- Campaign lifecycle invariants and application workflow invariants
- Shared financial calculator preserving the 25% platform fee and budget cap
- Request validation contracts and safe error types
- Email and cloud-storage abstractions
- Environment-safe configuration templates and deployment notes
- Placement activation/completion endpoints, including Published -> Active behavior
- PostgreSQL indexes/check constraints and a reviewed RLS policy foundation

## Next implementation slices

1. Generate and review the first EF Core migration against an isolated PostgreSQL instance
2. Authentication service, secure cookies, antiforgery, reset flow, Mailtrap adapter, and authorization policies
3. Campaign/community/application controllers and transactional services
4. Idempotent tracking click processing and ledger reconciliation
5. Health checks, exception middleware, rate limiting, security headers, structured logging, and API versioning
6. Admin endpoints, notifications, moderation, payouts, notes, and activity feed
7. Integration/security/financial tests and benchmark harness
8. Production migration, backup/restore, deployment, and rollback documentation

The frontend is intentionally not connected until these backend contracts are reviewed and tested.
