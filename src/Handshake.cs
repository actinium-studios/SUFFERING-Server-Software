using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using SUFFERING;

namespace SUFFERING_Server_Software;

public static class Handshake
{
    public static void Handle(ConnectionRequest req, bool serverFull)
    {
        int clientVersion;
        try
        {
            if (req.Data.GetString(32) != Protocol.Magic)
            {
                req.Reject();
                return;
            }
            clientVersion = req.Data.GetInt();
        }
        catch (Exception)
        {
            req.Reject();
            return;
        }

        if (clientVersion != Protocol.Version)
        {
            var reason = clientVersion < Protocol.Version
                ? RejectReason.ClientOutdated
                : RejectReason.ServerOutdated;

            Log.Information("Rejected {Addr}: client protocol {Client}, server protocol {Server}",
                req.RemoteEndPoint, clientVersion, Protocol.Version);
            RejectWith(req, reason);
            return;
        }

        if (serverFull)
        {
            RejectWith(req, RejectReason.ServerFull);
            return;
        }

        req.Accept();
    }

    private static void RejectWith(ConnectionRequest req, RejectReason reason)
    {
        var w = new NetDataWriter();
        w.Put((byte)reason);
        w.Put(Protocol.Version);
        w.Put(Protocol.GameVersion);
        req.Reject(w);
    }
}
