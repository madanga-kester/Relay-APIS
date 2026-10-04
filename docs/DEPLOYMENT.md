# Deployment runbook

Build the API with `dotnet publish src/Relay.Api -c Release -o publish`. Inject secrets through the host or secret manager; never commit `.env`, connection strings, SMTP credentials, S3 keys, or data-protection keys.

Deploy the database migration as a separate release step using the reviewed migration bundle. The API process should not run destructive or unreviewed migrations on startup. Run the readiness check before routing traffic: `GET /health/ready`. Liveness is `GET /health/live`.

Terminate TLS at the reverse proxy or load balancer and forward only trusted headers. Production cookies must remain Secure and HTTP-only. Swagger is development-only. Configure structured JSON logs, retention, and alerting for HTTP 5xx spikes, database readiness failures, budget reconciliation failures, and repeated tracking errors.

Production and staging must provide `Auth__DataProtectionCertificatePath` and its secret password through the host secret store. The API fails fast outside Development if the certificate is missing, preventing cookie/data-protection keys from being persisted unencrypted. Development may use local keys for convenience, but those keys must never be copied into production.

Back up PostgreSQL automatically with an agreed retention policy. At least monthly, restore a backup into an isolated staging database and verify the API can read it. For rollback, redeploy the previous immutable API artifact and use only backward-compatible database migrations; destructive schema changes require a separate expand/contract release.
