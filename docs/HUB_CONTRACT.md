# Hub Contract

Hub URL: `/hubs/game`. JSON is camelCase, enums are sent as strings.

A cell's chunk is `floor(cell / chunkSize)` on each axis.

## Authentication

The hub only accepts logged-in players. Login is stored in HttpOnly cookies, so the browser sends it by itself, but requests must include credentials (`credentials: "include"` for `fetch`, `withCredentials: true` for the SignalR connection).

Everyone starts as a guest. On page load, before connecting to the hub:

1. `GET /api/me`. If it returns 200, you are logged in, so connect.
2. On 401, `POST /api/refresh-token`. If it returns 200, connect.
3. On 401, `POST /api/guest`. It creates a guest player and sets the cookies, then connect.

A connection without a valid login is rejected with 401.

### Endpoints

| Endpoint | Purpose |
| --- | --- |
| `GET /api/me` | Who am I: `{ "id": "...", "userName": "Guest_...", "role": "Guest" }` |
| `POST /api/guest` | Create a guest player and log in as them |
| `POST /api/register` | Turn the current guest into a registered player. Body: `userName`, `password`, `confirmPassword`. The role becomes `Member` |
| `POST /api/login` | Log in to an existing registered account |
| `POST /api/refresh-token` | Get a new access token using the refresh cookie |
| `POST /api/logout` | Log out |

The access token is short-lived. When a request returns 401, call `/api/refresh-token`, then retry. A dropped hub connection should do the same before reconnecting.

## Plant types

The server sends only `plantTypeId`. The client picks the colors.

| Id | Name | Id | Name |
| --- | --- | --- | --- |
| 1 | Red | 9 | Dark Blue |
| 2 | Orange | 10 | Purple |
| 3 | Yellow | 11 | Pink |
| 4 | Light Green | 12 | Brown |
| 5 | Dark Green | 13 | White |
| 6 | Teal | 14 | Light Gray |
| 7 | Light Blue | 15 | Dark Gray |
| 8 | Blue | 16 | Black |

## Methods

### `GetGameConfig()`

Call it right after connecting, and again after every reconnect.

| Field | Meaning |
| --- | --- |
| `mapSize` | Width and height of the map in cells |
| `chunkSize` | Width and height of a chunk in cells |
| `harvestCost` | Energy cost of harvesting |
| `plantCost` | Energy cost of planting |
| `growthTimeSeconds` | Time for a plant to become ripe |
| `serverTimeUnixMilliseconds` | Server time, to correct the client clock: `offset = serverTime - Date.now()` |

### `LoadNearbyObjects(chunkX, chunkY)`

Returns the plants in that chunk and the chunks around it. Clipped at the map edges. Empty cells have no entry.

```json
{ "plants": [ { "x": 100, "y": 200, "plantTypeId": 3, "ripeAt": "2026-11-02T12:00:00Z" } ] }
```

A plant is ripe when `ripeAt` (UTC) has passed.

### `Plant(plantTypeId, cellX, cellY)`

```json
{ "outcome": "Success" }
```

Outcomes: `Success`, `CellOccupied`, `OutOfBounds`, `UnknownPlantType`.

### `Harvest(cellX, cellY)`

Removes the plant on a cell.

```json
{ "outcome": "Success" }
```

Outcomes: `Success`, `CellEmpty`, `OutOfBounds`.

## Events

### `WorldUpdated`

Sent to every connected client when a plant is planted or harvested. Register the handler before starting the connection.

The payload is an array of cells. `plant` has the same shape as the plants in `LoadNearbyObjects`, and is `null` when the cell is now empty.

```json
[
  { "x": 100, "y": 200, "plant": { "x": 100, "y": 200, "plantTypeId": 3, "ripeAt": "2026-11-02T12:00:00Z" } },
  { "x": 101, "y": 200, "plant": null }
]
```

Each entry says what the cell is now: set the cell to it. Applying the same update twice is harmless. The player who made the change receives it too.
