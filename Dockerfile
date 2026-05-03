# Build and publish MessageHawk.Api and MessageHawk.Worker.
# Targets: `api` (default), `worker`
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MessageHawk.slnx ./
COPY src ./src/

RUN dotnet publish src/MessageHawk.Api/MessageHawk.Api.csproj -c Release -o /app/publish/api
RUN dotnet publish src/MessageHawk.Worker/MessageHawk.Worker.csproj -c Release -o /app/publish/worker

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS worker
WORKDIR /app
COPY --from=build /app/publish/worker .
ENTRYPOINT ["dotnet", "MessageHawk.Worker.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish/api .
ENTRYPOINT ["dotnet", "MessageHawk.Api.dll"]
