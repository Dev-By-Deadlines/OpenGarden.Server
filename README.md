# StarterKit.Api

A minimal ASP.NET Core API starter focused on authentication and the infrastructure commonly needed by a new backend.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF%20Core-336791?logo=postgresql&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

- JWT authentication with HTTP-only cookies
- Refresh token rotation
- ASP.NET Core Identity
- PostgreSQL with Entity Framework Core
- FluentValidation
- Global IP-based rate limiting
- Serilog request logging
- Background cleanup of expired refresh tokens
- OpenAPI + Scalar API documentation
- Docker Compose for local PostgreSQL

## Tech Stack

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- JWT
- FluentValidation
- Serilog
- Scalar

## Project Structure

```text
├── Constants/       Application constants and roles
├── Data/            EF Core DbContext and configurations
├── Dtos/            Request DTOs
├── Endpoints/       API endpoints and handlers
├── Extensions/      ASP.NET Core extensions
├── Jobs/            Background services
├── Migrations/      EF Core migrations
├── Models/          Application entities
├── Services/        Application services
└── Validators/      FluentValidation validators
```

## Getting Started

### Requirements

- .NET 10 SDK
- Docker

### Start PostgreSQL

```bash
docker compose up -d
```

### Configure the application

Configure the connection string and JWT settings using local configuration or .NET User Secrets.

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=app;Username=app;Password=app;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "ExpirationInMinutes": 15
  }
}
```

Using User Secrets instead:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=app;Username=app;Password=app;"
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key"
dotnet user-secrets set "Jwt:Issuer" "your-issuer"
dotnet user-secrets set "Jwt:Audience" "your-audience"
```

### Run the API

```bash
dotnet run
```

Database migrations are applied automatically on startup.

When running in the Development environment, API documentation is available at:

```text
/docs
```

## Authentication

| Method | Endpoint              | Description                          |
| ------ | --------------------- | ------------------------------------- |
| `POST` | `/app/register`       | Register a new user                   |
| `POST` | `/app/login`          | Authenticate and issue tokens         |
| `POST` | `/app/refresh-token`  | Rotate the refresh token              |
| `POST` | `/app/logout`         | Log out                               |
| `GET`  | `/app/me`             | Get the authenticated user's claims   |

Access and refresh tokens are stored in HTTP-only cookies.

## Contributing

Issues and pull requests are welcome. If you're using this as a starter for your own project, feel free to fork and adapt it.

## License

This project is provided as a starter/template for personal and commercial projects.
