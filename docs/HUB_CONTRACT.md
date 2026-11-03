# Hub Contract

Hub URL: `/hubs/game`. JSON is camelCase, enums are sent as strings.

A cell's chunk is `floor(cell / chunkSize)` on each axis.

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
