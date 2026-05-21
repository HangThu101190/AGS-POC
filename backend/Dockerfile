# Build & run AGS SmartShift API (compose service: api)
# Build context: repo root (see production/docker-compose.yml) so embedded
# reference-data/Kế_hoạch_bay.xlsx resolves like local dev.
# TLS terminates at ingress/reverse proxy — container listens HTTP :8080
FROM mcr.microsoft.com/dotnet/sdk:8.0.411-bookworm-slim AS build
WORKDIR /repo
COPY production/backend/ ./production/backend/
COPY reference-data/ ./reference-data/
WORKDIR /repo/production/backend
RUN dotnet restore AGS.SmartShift.sln
RUN dotnet publish src/AGS.SmartShift.Api/AGS.SmartShift.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0.16-bookworm-slim AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --retries=5 --start-period=40s \
  CMD curl -f http://127.0.0.1:8080/health || exit 1
ENTRYPOINT ["dotnet", "AGS.SmartShift.Api.dll"]
