using Serilog;

namespace SUFFERING_Server_Software.Commands;

public sealed class KickCommand : ICommand
{
    public string Name => "kick";
    public string Description => "Disconnect a player by id";
    public string Usage => "kick <id>";

    public IEnumerable<string> Complete(ServerContext ctx, string[] previousArgs)
    {
        var ids = new List<string>();

        if (previousArgs.Length == 0)
            foreach (var p in ctx.Net)
                ids.Add(p.Id.ToString());

        return ids;
    }

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
