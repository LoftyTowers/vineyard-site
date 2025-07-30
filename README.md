# Vineyard API

This folder contains a minimal ASP.NET Core 9 API used by the Angular front end.

## Running the API

1. Install the .NET 9 SDK.
2. Restore packages and run the server:

```bash
dotnet restore
dotnet run --project VineyardApi
```

The API listens on `http://localhost:5212` by default. Use
`https://localhost:7170` for HTTPS.

## Entity Framework migrations

To create or apply migrations use the `dotnet-ef` tool:

```bash
dotnet ef migrations add <Name> --project VineyardApi
# Apply to the configured database
cd VineyardApi
dotnet ef database update
```

The connection string can be edited in `VineyardApi/appsettings.json`.
You can also set the environment variables `ConnectionStrings__DefaultConnection`
and `Jwt__Key` to override the database connection and JWT signing key at runtime.

## Images

Images are referenced by URL only. Upload files separately and store the
link in the `Images` table using the `/images` endpoint.

## Building the Angular site

The Angular application lives under `vineyard-site`. To build the front end:

```bash
cd vineyard-site
npm install
npx ng build
```

The optimized assets will be written to `vineyard-site/dist/`.

## Running with Docker Compose

You can spin up the API, database and Angular frontend using Docker Compose:

```bash
docker-compose up -d
```

The frontend is exposed on port `8080` by default. If port `80` is already in
use or requires elevated privileges, you can edit `docker-compose.yml` and
change the port mapping under the `frontend` service.

## Running tests

Unit tests exist for both the API and the Angular client.

### API tests

```bash
dotnet test Vineyard.sln
```

### Angular tests

These tests require a headless Chrome install. Run them with:

```bash
cd vineyard-site
npm install
CHROME_BIN=/usr/bin/chromium-browser npx ng test --watch=false
```

## Managing roles and logging in

Role assignments control access to the admin area. Insert roles and users
directly into the database, then associate them via the `UserRoles` table. A
password hash can be generated using `BCrypt.Net.BCrypt.HashPassword("password")`.

```sql
INSERT INTO "Roles" ("Name") VALUES ('Admin');
INSERT INTO "Users" ("Id", "Username", "PasswordHash", "Email", "CreatedAt", "IsActive")
VALUES (gen_random_uuid(), 'admin', '<hash>', 'admin@example.com', NOW(), TRUE);
INSERT INTO "UserRoles" ("UserId", "RoleId") VALUES (<user_id>, <role_id>);
```

Authenticate by POSTing to `/auth/login` with the username and password to
receive a JWT token. Include this token in the `Authorization` header when
visiting `/admin/*` routes in the Angular site.
