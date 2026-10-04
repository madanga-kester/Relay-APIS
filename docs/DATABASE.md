# PostgreSQL database notes

The EF Core model is the source for the relational schema. The first migration should be generated only after reviewing the model and setting `ConnectionStrings__Postgres`:

```bash
export PATH="$HOME/.dotnet:$PATH"
cd backend
dotnet tool install --global dotnet-ef --version 8.0.11
dotnet ef migrations add InitialMarketplace --project src/Relay.Infrastructure --startup-project src/Relay.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/Relay.Infrastructure --startup-project src/Relay.Api
```

Production deployments must run `dotnet ef database update` as a controlled release step, not automatically from the web process. RLS policies will be added in a reviewed SQL migration after the application-level authorization queries are in place.
