using System.Diagnostics;
using Serilog;

namespace SUFFERING_Server_Software.Commands;

public sealed class StatusCommand : ICommand
{
    public string Name => "status";
    public string Description => "Show uptime and player count";

    public void Execute(ServerContext ctx, string[] args) =>
        Log.Information("Uptime {Up:hh\\:mm\\:ss}, {N}/{Max} players",
            Stopwatch.GetElapsedTime(ctx.StartedAt), ctx.Net.ConnectedPeersCount, ctx.Config.MaxPlayers);
}
