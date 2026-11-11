using Serilog;

namespace SUFFERING_Server_Software.Commands;

public sealed class HelpCommand : ICommand
{
    public string Name => "help";
    public string Description => "List all commands";

    public void Execute(ServerContext ctx, string[] args)
    {
        foreach (var c in ctx.Commands.All)
            Log.Information("{Usage,-14} {Description}", c.Usage, c.Description);
    }
}
