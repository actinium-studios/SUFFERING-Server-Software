# syntax=docker/dockerfile:1

# ---- build ----
# Runs on the build machine's own architecture and cross-publishes for the target,
# so arm64 images build quickly without emulating the .NET SDK.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

# Restore first so this layer is cached until the csproj or package config changes
COPY SUFFERING_Server_Software.csproj version.txt nuget.config* packages.lock.json* ./
RUN dotnet restore -a $TARGETARCH

COPY src ./src
RUN dotnet publish -c Release -a $TARGETARCH --no-restore \
    -p:UseAppHost=false -p:DebugType=None \
    -o /app

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# The server reads config.yaml and writes logs/ relative to its working directory,
# so /data holds everything that should persist.
RUN mkdir /data && chown $APP_UID:$APP_UID /data
WORKDIR /data
USER $APP_UID
VOLUME /data

EXPOSE 25666/udp
ENTRYPOINT ["dotnet", "/app/SufferingServer.dll"]
