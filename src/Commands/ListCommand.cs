using Serilog;

namespace SUFFERING_Server_Software.Commands;

public sealed class ListCommand : ICommand
{
    public string Name => "list";
    public string Description => "List connected players";

    public void Execute(ServerContext ctx, string[] args)
    {
        Log.Information("{N} player(s) online", ctx.Net.ConnectedPeersCount);

        foreach (var p in ctx.Net)
            Log.Information("  #{Id} {Addr}", p.Id, p.Address);
    }
}
