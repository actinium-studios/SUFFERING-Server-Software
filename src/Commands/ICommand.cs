namespace SUFFERING_Server_Software.Commands;
 
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string Usage => Name;
    IReadOnlyList<string> Aliases => [];
    void Execute(ServerContext ctx, string[] args);
}
 
 