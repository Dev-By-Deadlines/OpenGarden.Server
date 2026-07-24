# AGENTS.md

.NET 10.0 ASP.NET Core Minimal API starter template. Single project, no solution file.

## Commands

- `dotnet build` — build
- `dotnet run` — dev server on `http://localhost:5002` (see `Properties/launchSettings.json`)
- `dotnet watch run` — hot reload
- `docker compose up -d` — starts PostgreSQL (user/pass/db: `app`/`app`/`app`, port 5432)

No test project exists. No linter or formatter configured beyond .NET defaults.

## Architecture

```
Program.cs                    — entry point, wires everything
Endpoints/StarterEndpoints.cs — route definitions (group: /app)
Endpoints/Handlers/           — handler classes (Register, Login)
Data/AppDbContext.cs           — IdentityDbContext, schema: "identity"
Models/                        — ApplicationUser, TestModel
Dtos/                          — plain records (no validation)
Constants/Roles.cs             — "Admin", "Member"
Migrations/                    — EF Core migrations (auto-applied on startup)
```

## Gotchas

- **Migrations auto-apply**: `Program.cs` calls `db.Database.Migrate()` on startup. Don't manually run migrations unless debugging.
- **JWT secret required**: `Jwt:SecretKey` must be set in user secrets or env vars. Not in `appsettings.json`. App will throw on login without it.
- **No request validation**: DTOs are bare records. FluentValidation not yet added.
- **No refresh tokens**: Only short-lived access tokens (2 min). Refresh token flow not implemented.
- **Scalar, not Swagger**: API docs at `/docs` in dev mode via Scalar.
- **Schema split**: Identity tables in `identity` schema, domain tables in `public`.
- **Secrets excluded from git**: `appsettings.Development.json` and `appsettings.Local.json` are gitignored.

## Conventions

- Minimal API style with handler classes per endpoint
- File-scoped namespaces, primary constructors where applicable
- Conventional commits (`feat:`, `fix:`, `refactor:`, `chore:`, etc.)
- Work directly on `main` unless risky
- Update this file when features change significantly
- Do not write code or generate full solutions unless explicitly asked. Prefer explaining, suggesting, and pointing to what to look up or create next.

## Remaining roadmap

- [ ] Request validation (FluentValidation)
- [ ] Refresh token flow
- [ ] Health endpoint (`/health`)
- [ ] Better error handling / result pattern
