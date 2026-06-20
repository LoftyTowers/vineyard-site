# Vineyard Website and Content Management System

A full-stack proof of concept for one vineyard within a wider co-operative, built with Angular, ASP.NET Core 9 and PostgreSQL. The project explores how an individual vineyard can manage its own content and identity while still following a shared visual structure that could later be reused across the group.

## Architecture and Technical Decisions

This project focuses on delivering a complete website and content-management system for a single vineyard. It allows the vineyard to manage its own pages, images, people and published content without requiring code changes for routine updates. Although the current implementation serves one vineyard, it was designed as a proof of concept for a wider co-operative model in which each vineyard could have its own content and identity while remaining visually consistent with the rest of the group.

Angular was selected for its component-based architecture, making it easier to create reusable page layouts and administration tools while keeping the frontend flexible. ASP.NET Core provides a strongly typed and testable API for authentication, content management and business rules. PostgreSQL was chosen for reliable relational storage, with JSONB used where page content needs greater flexibility. JWT authentication keeps the API stateless and protects the administration area. Docker Compose runs the Angular frontend, .NET API and PostgreSQL database as a consistent stack, with separate development, staging and production configurations to reduce environment-specific issues and support a controlled release process.


## Contribution guidelines

- PR template and expectations: `docs/pr-guidelines.md`
- Commit message rules: `docs/commit-messages.md`

## Local Development

### Run the API

1. Install the .NET 9 SDK.
2. Restore packages and run the server:

```bash
dotnet restore
dotnet run --project VineyardApi
```

The API listens on `http://localhost:5212` by default. Use
`https://localhost:7170` for HTTPS.

Windows users may need to fix line endings for `VineyardApi/entrypoint.sh`.
After pulling this repository with the new `.gitattributes` file, re-clone the
repo or run the following commands to reset the file with LF endings:

```bash
git rm --cached VineyardApi/entrypoint.sh
git checkout -- VineyardApi/entrypoint.sh
```

### Run the Angular client

The Angular application lives under `vineyard-site`. Start the development
server with:

```bash
cd vineyard-site
npm install
npm start
```

The client will proxy API requests to the backend and serve the site on
`http://localhost:4200`.

### Migrations and seeding

Use the `dotnet-ef` tool to manage migrations and apply them to your local
database:

```bash
dotnet ef migrations add <Name> --project VineyardApi
cd VineyardApi
dotnet ef database update
```

After applying migrations, seed the database with default content:

```bash
psql -f SeedScripts/initial_seed.sql
```

### Run unit tests

API tests:

```bash
dotnet test Vineyard.sln
```

Angular tests (requires a headless Chrome install):

```bash
cd vineyard-site
npm install
CHROME_BIN=/usr/bin/chromium-browser npx ng test --watch=false
```

## Environment variables

The API reads sensitive settings from environment variables:

- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string.
- `Jwt__Key` - JWT signing key.
- `SUPERADMIN_EMAIL` - email for the initial super admin user.
- `SUPERADMIN_PASSWORD` - password for the super admin user.

## Images

Images are referenced by URL only. Upload files separately and store the
link in the `Images` table using the `/images` endpoint.

## Staging/Test Deployment

Use the test configuration to spin up the full stack with prebuilt images. The
`.env.staging` file supplies credentials and connection strings.

```bash
docker compose --env-file .env.staging -f docker-compose.yml -f docker-compose.staging.yml up -d
# Apply migrations and seed data inside the running containers
docker compose exec api dotnet ef database update
cat SeedScripts/initial_seed.sql | docker compose exec -T db psql -U $POSTGRES_USER -d $POSTGRES_DB
```

The stack exposes the Angular site on port `8080`. Adjust the port mapping in
`docker-compose.yml` if needed.

## Production Deployment

For production, use `.env.production` with the production override file:

```bash
docker compose --env-file .env.production -f docker-compose.yml -f docker-compose.prod.yml up -d
docker compose exec api dotnet ef database update
cat SeedScripts/initial_seed.sql | docker compose exec -T db psql -U $POSTGRES_USER -d $POSTGRES_DB
```

Best practices:

- Run the stack behind a reverse proxy such as Nginx or Traefik.
- Enable TLS and automate certificate renewal (e.g., with Let's Encrypt).
- Schedule regular backups of the `db_data` volume and `.env.production` file.

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

