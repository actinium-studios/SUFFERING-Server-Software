namespace SUFFERING_Server_Software.Commands;

public sealed class StopCommand : ICommand
{
    public string Name => "stop";
    public string Description => "Stop the server";
    public IReadOnlyList<string> Aliases => ["exit", "quit"];

    public void Execute(ServerContext ctx, string[] args) => ctx.RequestStop();
}
