# Running the server locally

You need Docker (with Docker Compose). You don't need .NET or the source code.

## Start

```bash
cd dev
docker compose pull
docker compose up -d
```

The game hub is at `http://localhost:5000/hubs/game`.

## Update to the latest server version

```bash
docker compose pull
docker compose up -d
```

## Stop

```bash
docker compose down        # stop, keep the data
docker compose down -v     # stop and wipe the database
```

## Useful

```bash
docker compose logs -f server    # follow the server log
```
