using System.Reflection;

namespace SUFFERING;

public static class Protocol
{
    public const string Magic = "SUFFERING";

    public const int Version = 1;

    public static readonly string GameVersion = ReadGameVersion();

    private static string ReadGameVersion()
    {
        var info = typeof(Protocol).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(info)) return "unknown";

        var plus = info.IndexOf('+');
        return plus >= 0 ? info[..plus] : info;
    }
}

public enum RejectReason : byte
{
    ClientOutdated = 1,
    ServerOutdated = 2,
    ServerFull = 3,
}
