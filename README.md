# 🌱 OpenGarden

[![CI](https://github.com/dev-by-deadlines/OpenGarden.Server/actions/workflows/ci.yml/badge.svg)](https://github.com/dev-by-deadlines/OpenGarden.Server/actions/workflows/ci.yml)

**r/place, but every pixel is a plant.**

One shared grid. Harvest plants to get seeds, plant seeds to paint the map together, or tear down someone else's art. Energy limits what you can do, and nobody owns anything.

> 🚧 **Early development.** The server is working (auth, planting, harvesting, energy, real-time updates).

## How it plays

1. **Jump in as a guest.** No signup, you get a plot of the world and some starter seeds.
2. **Harvest** plants to collect more seeds.
3. **Plant** seeds to paint the shared map.
4. **Watch your energy.** Every action costs it, so spend it wisely.
5. **Register** whenever you want to keep your progress.

## Built with

ASP.NET Core · SignalR · PostgreSQL · Entity Framework Core

Tested with xUnit and Testcontainers against a real PostgreSQL instance. GitHub Actions runs the tests on every push and publishes the server image to GitHub Container Registry.

## Docs

- [Auth Contract](docs/AUTH_CONTRACT.md): How login, guest accounts, and token refresh work.
- [Hub Contract](docs/HUB_CONTRACT.md): SignalR methods, events, and data structures.
- [Development Guide](dev/README.md): Running the published server image and dev setup.

## Run it

Want to try the server without building it? Follow the [Development Guide](dev/README.md).
