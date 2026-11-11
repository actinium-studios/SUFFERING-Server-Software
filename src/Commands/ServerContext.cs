using LiteNetLib;
 
namespace SUFFERING_Server_Software.Commands;

public sealed record ServerContext(
    ServerConfig Config,
    NetManager Net,
    CommandRegistry Commands,
    Action RequestStop,
    long StartedAt);
 
