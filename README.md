# SUFFERING-Server-Software
Yes its exactly what it sounds like :3


## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git, when building from source

## Build from source

Clone the repository and build the project in Release configuration:

```bash
git clone https://github.com/actinium-studios/SUFFERING-Server-Software.git
cd SUFFERING-Server-Software
dotnet restore
dotnet build --configuration Release
```

The compiled assembly is written to `bin/Release/net10.0/`.

## Run from source

Run the server directly with the .NET SDK:

```bash
dotnet run --configuration Release
```

## Create a standalone executable

The following commands publish a self-contained, single-file executable for
each supported Linux or Windows architecture:

```bash
dotnet publish SUFFERING-Server-Software.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:DebugType=None \
  --output publish/linux-x64
```

Replace `linux-x64` with `linux-arm64`, `win-x64`, or `win-arm64` as needed.
The executable is output as `publish/<runtime>/SufferingServer` on Linux or
`publish/<runtime>/SufferingServer.exe` on Windows.

macOS releases are provided as a universal `.app` bundle containing both
`osx-x64` and `osx-arm64` binaries. The release workflow creates this bundle
on a macOS runner.

## Download a release

Prebuilt release archives are available on the repository's
[Releases page](https://github.com/actinium-studios/SUFFERING-Server-Software/releases).
Choose the archive matching your operating system and CPU architecture:

- `SufferingServer-linux-x64.tar.gz`
- `SufferingServer-linux-arm64.tar.gz`
- `SufferingServer-win-x64.zip`
- `SufferingServer-win-arm64.zip`
- `SufferingServer-macos-universal.zip`
