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

## Energy and seeds

- Planting and harvesting both cost energy (`plantCost`, `harvestCost` from `GetGameConfig`). Energy regenerates over time, and guests have less than registered players.
- Planting also uses one seed of the chosen plant type. Harvesting gives the player seeds of the harvested plant's type.
- A new player starts with some seeds of every plant type.
- A refused action costs nothing.
- To display energy, take `energy` from the response, remember when it arrived, and add one point per `energyRegenSeconds` elapsed, up to `maxEnergy`.

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

## Player state

### `PlayerState`

The player's current energy and seed inventory.

| Field | Type | Meaning |
| --- | --- | --- |
| `energy` | `int` | Current energy |
| `maxEnergy` | `int` | Maximum energy |
| `energyRegenSeconds` | `int` | Seconds required to regenerate one energy point |
| `seeds` | `SeedStack[]` | Seed inventory, grouped by plant type |

Each seed stack has this shape:

```json
{
  "plantTypeId": 3,
  "quantity": 10
}
```

### `GetPlayerState()`

Returns the current player's state.

```json
{
  "energy": 50,
  "maxEnergy": 100,
  "energyRegenSeconds": 60,
  "seeds": [
    { "plantTypeId": 1, "quantity": 10 },
    { "plantTypeId": 3, "quantity": 15 }
  ]
}
```

The return type is nullable (`PlayerStateDto?`).

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

### `GetPlayerState()`

Returns the current player's `PlayerState` (see above).

Call it when connecting or reconnecting to obtain the latest player state.

### `LoadNearbyObjects(chunkX, chunkY)`

Returns the plants in that chunk and the chunks around it. Clipped at the map edges. Empty cells have no entry.

```json
{
  "plants": [
    {
      "x": 100,
      "y": 200,
      "plantTypeId": 3,
      "ripeAt": "2026-11-02T12:00:00Z"
    }
  ]
}
```

A plant is ripe when `ripeAt` (UTC) has passed.

### `Plant(plantTypeId, cellX, cellY)`

Plants a sprout on an empty cell. Costs `plantCost` energy and one seed of `plantTypeId`.

```json
{
  "outcome": "Success",
  "playerState": {
    "energy": 40,
    "maxEnergy": 100,
    "energyRegenSeconds": 60,
    "seeds": [
      { "plantTypeId": 3, "quantity": 9 }
    ]
  }
}
```

Outcomes: `Success`, `CellOccupied`, `OutOfBounds`, `UnknownPlantType`, `NotEnoughEnergy`, `NoSeeds`.

The response includes the player's updated state.

### `Harvest(cellX, cellY)`

Removes the plant on a cell. Costs `harvestCost` energy and gives seeds of the harvested plant's type.

```json
{
  "outcome": "Success",
  "plantTypeId": 3,
  "seedsGained": 5,
  "playerState": {
    "energy": 30,
    "maxEnergy": 100,
    "energyRegenSeconds": 60,
    "seeds": [
      { "plantTypeId": 3, "quantity": 14 }
    ]
  }
}
```

| Field | Meaning |
| --- | --- |
| `outcome` | Result of the harvest action |
| `plantTypeId` | Type of the harvested plant; nullable |
| `seedsGained` | Number of seeds gained |
| `playerState` | Updated player state |

Outcomes: `Success`, `CellEmpty`, `OutOfBounds`, `NotEnoughEnergy`.

The response includes the harvested plant's type, the number of seeds gained, and the player's updated state.

## Events

### `WorldUpdated`

Sent to every connected client when a plant is planted, harvested or spawned by the server. Register the handler before starting the connection.

The payload is an array of cells. `plant` has the same shape as the plants in `LoadNearbyObjects`, and is `null` when the cell is now empty.

```json
[
  {
    "x": 100,
    "y": 200,
    "plant": {
      "x": 100,
      "y": 200,
      "plantTypeId": 3,
      "ripeAt": "2026-11-02T12:00:00Z"
    }
  },
  {
    "x": 101,
    "y": 200,
    "plant": null
  }
]
```

Each entry says what the cell is now: set the cell to it. Applying the same update twice is harmless. The player who made the change receives it too.
