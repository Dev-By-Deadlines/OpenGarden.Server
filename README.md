# OpenGarden

[![CI](https://github.com/dev-by-deadlines/OpenGarden.Server/actions/workflows/ci.yml/badge.svg)](https://github.com/dev-by-deadlines/OpenGarden.Server/actions/workflows/ci.yml)

A multiplayer browser game in the spirit of r/place, where every pixel is a plant. Players share one grid map, harvest plants to get seeds, and plant seeds to paint it together, or tear each other's art down. Energy limits what each player can do, and nobody owns anything.

**Status:** Early development. The server supports player authentication, energy and seed management, planting, harvesting, map loading, and real-time world updates. The browser client has not started development yet.

## What's inside

- **Server:** ASP.NET Core, SignalR, PostgreSQL, Entity Framework Core
- **Authentication:** Guest accounts, registration, login, and refresh tokens
- **Game systems:** Planting, harvesting, seeds, and energy regeneration
- **Real-time communication:** SignalR hub for multiplayer world updates
- **Tests:** xUnit unit and integration tests against a real PostgreSQL instance using Testcontainers
- **CI/CD:** GitHub Actions runs tests on every push and publishes the server image to GitHub Container Registry

## Documentation

- [Hub Contract](docs/HUB_CONTRACT.md): Hub methods, events, authentication requirements, and data structures.
- [Development Guide](dev/README.md): Running the published server image and development setup.

## Run it

To run the published server image instead of building from source, see the [Development Guide](dev/README.md).
