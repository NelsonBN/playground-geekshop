# syntax=docker/dockerfile:1.10

###############################################################################
# Runtime base: Alpine, pinned, non-root, with ICU (required by Microsoft.Data.SqlClient)
###############################################################################
FROM mcr.microsoft.com/dotnet/aspnet:11.0.0-alpine3.24 AS base-env

WORKDIR /app
EXPOSE 8080

# Install what Microsoft.Data.SqlClient needs for globalization/time zones, without leaving the apk cache in the layer
RUN apk add --no-cache \
        icu-libs \
        icu-data-full \
        tzdata

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://*:8080 \
    ASPNETCORE_HTTP_PORTS= \
    ASPNETCORE_HTTPS_PORTS=

# Healthy only when the API answers and can reach SQL Server (readiness probe, see Setup/HealthSetup.cs)
HEALTHCHECK --interval=10s \
            --timeout=5s \
            --start-period=10s \
            --retries=5 \
    CMD wget --quiet --tries=1 -O /dev/null http://localhost:8080/healthz/ready || exit 1

# The aspnet image ships a non-root "app" user
USER app



###############################################################################
# Build: restore (cached while only sources change), then publish
###############################################################################
# Always build on the host's native platform: the published output is architecture-neutral (UseAppHost=false), so multi-arch builds avoid emulation
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0.100-alpine3.24 AS build-env

# NUGET_XMLDOC_MODE=skip: skip extraction of XML docs, not useful inside an image and speeds up restore
# NUGET_PACKAGES: fixed location so it can be targeted by the BuildKit cache mount
ENV DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    NUGET_XMLDOC_MODE=skip \
    NUGET_PACKAGES=/nuget/packages

WORKDIR /src

# Copy just the project file and restore, so this layer is reused until the dependencies change
# The NuGet cache mount persists downloaded packages across builds, even when the layer is invalidated
COPY --link ./src/GeekShop.Api/*.csproj ./GeekShop.Api/
RUN --mount=type=cache,id=nuget,target=/nuget/packages \
    dotnet restore ./GeekShop.Api/GeekShop.Api.csproj

# Copy the sources and publish. "--no-restore" keeps the restore layer cache (the cache mount is needed again to resolve the packages)
COPY --link ./src/GeekShop.Api/ ./GeekShop.Api/
RUN --mount=type=cache,id=nuget,target=/nuget/packages \
    dotnet publish ./GeekShop.Api/GeekShop.Api.csproj \
        --configuration Release \
        --no-restore \
        -p:UseAppHost=false \
        --output /publish



###############################################################################
# Final image: runtime base + the published output only
###############################################################################
FROM base-env AS run-env

COPY --link --from=build-env /publish .

ENTRYPOINT ["dotnet", "GeekShop.Api.dll"]
