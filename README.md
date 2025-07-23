# Vineyard API

This folder contains a minimal ASP.NET Core 9 API used by the Angular front end.

## Running the API

1. Install the .NET 9 SDK.
2. Restore packages and run the server:

```bash
dotnet restore
dotnet run --project VineyardApi
```

The API listens on `https://localhost:5001` by default.

## Entity Framework migrations

To create or apply migrations use the `dotnet-ef` tool:

```bash
dotnet ef migrations add <Name> --project VineyardApi
# Apply to the configured database
cd VineyardApi
dotnet ef database update
```

The connection string can be edited in `VineyardApi/appsettings.json`.

## Images

Images are referenced by URL only. Upload files separately and store the
link in the `Images` table using the `/images` endpoint.
