#!/bin/bash
set -e

echo "Starting API and applying migrations..."
dotnet VineyardApi.dll
