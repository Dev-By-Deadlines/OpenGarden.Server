## Login

POST /api/login

Authenticates a registered player using their username and password.

Returns:
- 200 OK: Authentication successful. Access and refresh token cookies are set.
- 401 Unauthorized: Authentication failed. This includes invalid credentials,
  nonexistent users, guest accounts, and locked accounts.

Account lockout:
- After 5 consecutive failed login attempts, the account is locked for 15 minutes.
- Successful authentication resets the failed login counter.
- Login attempts while locked out return 401 Unauthorized.
