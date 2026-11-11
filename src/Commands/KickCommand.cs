using Serilog;

namespace SUFFERING_Server_Software.Commands;

public sealed class KickCommand : ICommand
{
    public string Name => "kick";
    public string Description => "Disconnect a player by id";
    public string Usage => "kick <id>";

    public void Execute(ServerContext ctx, string[] args)
    {
        if (args.Length > 0 && int.TryParse(args[0], out var id) && ctx.Net.GetPeerById(id) is { } peer)
        {
            ctx.Net.DisconnectPeer(peer);
            Log.Information("Kicked #{Id}", id);
        }
        else
        {
            Log.Warning("Usage: {Usage}", Usage);
        }
    }
}
