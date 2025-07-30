# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore VineyardApi.csproj
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
# Copy source for migrations
COPY --from=build /src ./VineyardApi
# Install EF tool
RUN dotnet tool install --global dotnet-ef --version 9.* --no-cache
ENV PATH="/root/.dotnet/tools:$PATH"
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
EXPOSE 5212
ENTRYPOINT ["/entrypoint.sh"]
