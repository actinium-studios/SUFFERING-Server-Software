using System.Runtime.InteropServices;
using Serilog;
using SUFFERING;

namespace SUFFERING_Server_Software.Commands;

public sealed class VersionCommand : ICommand
{
    public string Name => "version";
    public string Description => "Show server version information";
    public IReadOnlyList<string> Aliases => ["ver"];

    public void Execute(ServerContext ctx, string[] args)
    {
        Log.Information("SUFFERING Server {Version} (protocol {Protocol})", Protocol.GameVersion, Protocol.Version);
        Log.Information("{Runtime} on {Os} {Arch}",
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription,
            RuntimeInformation.ProcessArchitecture);
    }
}
