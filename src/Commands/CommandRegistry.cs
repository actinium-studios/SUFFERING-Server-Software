using System.Reflection;

namespace SUFFERING_Server_Software.Commands;

public sealed class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _byName = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> Names => _byName.Keys;

    public IEnumerable<ICommand> All => _byName.Values.Distinct().OrderBy(c => c.Name);

    public void Register(ICommand command)
    {
        _byName[command.Name] = command;
        foreach (var alias in command.Aliases)
            _byName[alias] = command;
    }

    public void RegisterAllFrom(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type is { IsClass: true, IsAbstract: false }
                && typeof(ICommand).IsAssignableFrom(type)
                && type.GetConstructor(Type.EmptyTypes) is not null)
            {
                Register((ICommand)Activator.CreateInstance(type)!);
            }
        }
    }

    public bool TryGet(string name, out ICommand command) => _byName.TryGetValue(name, out command!);
}
