# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a minimal **.NET 10.0 ASP.NET Core Web API** starter project using the Minimal API style. It is a single-project setup with no solution file.

## Build & Run Commands

- **Build**: `dotnet build`
- **Run (development)**: `dotnet run` — runs on `http://localhost:5008`
- **Run with HTTPS**: `dotnet run --launch-profile https` — runs on `https://localhost:7189` and `http://localhost:5008`
- **Watch mode**: `dotnet watch run`
- **Restore packages**: `dotnet restore`
- **Clean**: `dotnet clean`

## Project Structure

- `Program.cs` — Entry point with a single minimal API endpoint (`GET /` returns "Hello World!").
- `StarterKit.Api.csproj` — Project file targeting `net10.0` with `Nullable` and `ImplicitUsings` enabled.
- `appsettings*.json` — Standard ASP.NET Core configuration files.
- `Properties/launchSettings.json` — Launch profiles for `http` (port 5008) and `https` (port 7189).
- `starter.http` — Simple HTTP request for manual API testing.
- `.gitignore` — Standard .NET gitignore (excludes `bin/`, `obj/`, `*.db`, editor/OS files, publish output; also excludes `appsettings.Development.json` and `appsettings.Local.json`).

## Goal: Template Project Roadmap

This project is a **work-in-progress starter template** designed to be cloned/reused as the base for new minimal API projects. The author is learning and building it incrementally. Planned features to add:

1. **ASP.NET Core + EF Core + PostgreSQL** — data persistence using Entity Framework Core with PostgreSQL
2. **Swagger** — API documentation (OpenAPI/Swagger UI)
3. **Serilog** — structured logging
4. **CORS** — Cross-origin resource sharing configuration
5. **Rate limiting** — request throttling
6. **Identity + JWT + refresh tokens** — authentication and authorization
7. **Health endpoint** — application health checks (`/health` or similar)
8. **FluentValidation** — request model validation

## Communication Style

The author is a beginner/intermediate developer learning by building. **Do not write code or generate full solutions unless explicitly asked.** Prefer explaining concepts, giving step-by-step directions, pointing to relevant documentation, and suggesting what to look up or create next.

## Key Configuration

- `Nullable` and `ImplicitUsings` are enabled in the `.csproj` — new projects should follow this pattern.
- The `.gitignore` intentionally ignores `appsettings.Development.json` and `appsettings.Local.json` to prevent secrets from being committed.
- There are currently no NuGet packages installed (beyond `Microsoft.NET.Sdk.Web`).
