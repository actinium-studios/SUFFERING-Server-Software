using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SUFFERING_Server_Software;

public sealed class ServerConfig
{
    public int Port { get; set; } = 25666;
    public int TickRate { get; set; } = 20;
    public int MaxPlayers { get; set; } = 20;
    public string Motd { get; set; } = "SUFFERING Server";

    public static ServerConfig LoadOrCreate(string path)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, Template(new ServerConfig()));
            return new ServerConfig();
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        ServerConfig cfg;
        try
        {
            cfg = deserializer.Deserialize<ServerConfig?>(File.ReadAllText(path)) ?? new ServerConfig();
        }
        catch (YamlException ex)
        {
            throw new InvalidOperationException($"{path} (line {ex.Start.Line}): {ex.InnerException?.Message ?? ex.Message}", ex);
        }

        cfg.Validate(path);
        return cfg;
    }

    private void Validate(string path)
    {
        if (Port is < 1 or > 65535)
            throw new InvalidOperationException($"{path}: port must be between 1 and 65535 (got {Port})");
        if (TickRate is < 1 or > 1000)
            throw new InvalidOperationException($"{path}: tick_rate must be between 1 and 1000 (got {TickRate})");
        if (MaxPlayers < 1)
            throw new InvalidOperationException($"{path}: max_players must be at least 1 (got {MaxPlayers})");
    }

    private static string Template(ServerConfig d) => $"""
        # SUFFERING server configuration
        # If you don't know what a config does it is probably not for you to touch, buddy.
        # Please restart the server after editing this file.

        # UDP port the server listens on (forward UDP, not TCP)
        port: {d.Port}

        # Simulation ticks per second
        tick_rate: {d.TickRate}

        # Maximum number of connected players
        max_players: {d.MaxPlayers}

        # Message of the day
        motd: "{d.Motd}"
        """ + "\n";
}
