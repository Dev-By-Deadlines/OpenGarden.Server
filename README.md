# OpenGarden

[![CI](https://github.com/dev-by-deadlines/OpenGarden.Server/actions/workflows/ci.yml/badge.svg)](https://github.com/dev-by-deadlines/OpenGarden.Server/actions/workflows/ci.yml)

A multiplayer browser game in the spirit of r/place, where every pixel is a plant. Players share one grid map, harvest plants to get seeds, and plant seeds to paint it together, or tear each other's art down. Energy limits what each player can do, and nobody owns anything.

**Status:** early development. The server can load the map, plant, harvest and push live updates to every connected player. Seeds, energy and player accounts are being built, and the browser client is not started yet.

## What's inside

- **Server:** .NET, SignalR, PostgreSQL (EF Core)
- **Tests:** xUnit unit tests and integration tests that run the real app against a Postgres container (Testcontainers)
- **CI:** GitHub Actions runs the tests on every push and publishes the server image to GitHub Container Registry
- **Contract:** the hub methods and data shapes are described in [`HUB_CONTRACT.md`](HUB_CONTRACT.md)

## Run it

You need Docker.

```bash
docker compose up --build
```

The game hub is then at `http://localhost:5000/hubs/game`.

To run the published server image instead of building from source, see [`dev/README.md`](dev/README.md).
