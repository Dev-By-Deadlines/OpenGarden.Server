# AUTH_CONTRACT

## How it works

Auth uses two HttpOnly cookies set by the server. You can't read or write them from JS.

- `accessToken`: sent with every request, lasts about 10 minutes
- `refreshToken`: used only to get a new pair, lasts about 7 days

Lifetimes are server config and may change, so don't hardcode them.

Send every request with credentials (`credentials: "include"` / `withCredentials: true`).

## Endpoints

| Endpoint | Auth needed | Success | Errors |
|---|---|---|---|
| `GET /api/me` | yes | 200 `{ id, userName, role }` where role is `Guest`, `Member` or `Admin` | 401 |
| `POST /api/guest` | no | 200, cookies set | 409 name collision (retry once), 429 rate limited |
| `POST /api/register` | yes | 200, new cookies set | 400 validation, 401, 409 already registered or username taken |
| `POST /api/login` | no | 200, cookies set | 401 (see below) |
| `POST /api/refresh-token` | no | 200, new cookies set | 401 session is over |
| `POST /api/logout` | no | 200 always, cookies cleared | none |

Notes:
- **Guest:** creates a new player every call. Only call it when there is no session.
- **Register:** upgrades the current guest to a member and keeps progress. No login needed afterwards. Password minimum is 8 characters.
- **Login:** 401 covers wrong password, unknown user, guest account and locked account. Show one generic error. 5 failed attempts lock the account for 15 minutes.
- **Refresh:** tokens are single use. Never run two refreshes in parallel, the second one will fail with 401. Queue other requests until it finishes.
- **Logout:** works even with an expired access token. Warn guests first, since a guest can't get their account back.

## Frontend flow

1. On app start, call `GET /api/me`. If 200, you're logged in.
2. On 401, call `POST /api/refresh-token` once. If 200, retry the original request.
3. If refresh returns 401, there is no session. Show the entry screen: "Play as guest" or "Log in".
4. Any 401 during normal use goes through step 2. If the retry still fails, go to step 3. Never loop.

## When the user must log in

- **Access token expired:** nothing, refresh happens silently.
- **Refresh token expired or cleared (7 days inactive, logout, cleared cookies):** members log in again. Guests lose their progress and get a new guest account.
- **Guest who wants to keep progress:** register before logging out.

## Rate limits

Per IP, 1-minute window, 429 when exceeded: 100 requests overall, 2 for `POST /api/guest`.
