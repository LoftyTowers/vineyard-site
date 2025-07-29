#!/bin/sh
set -e

# Apply database migrations if available
if [ "$SKIP_MIGRATIONS" != "true" ]; then
  dotnet ef database update --project ./VineyardApi/VineyardApi.csproj --no-build
fi

exec dotnet VineyardApi.dll
