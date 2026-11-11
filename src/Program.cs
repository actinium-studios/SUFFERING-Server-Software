using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using LiteNetLib;
using Serilog;
using SUFFERING_Server_Software;
using SUFFERING_Server_Software.Commands;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u4}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/latest.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

ServerConfig config;
try
{
    config = ServerConfig.LoadOrCreate("config.yaml");
}
catch (Exception ex)
{
    Log.Fatal("Invalid configuration: {Message}", ex.Message);
    await Log.CloseAndFlushAsync();
    return 1;
}
var started = Stopwatch.GetTimestamp();
using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
using var sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, sig =>
{
    sig.Cancel = true;
    cts.Cancel();
});

var listener = new EventBasedNetListener();
var net = new NetManager(listener);

listener.ConnectionRequestEvent += req =>
{
    if (net.ConnectedPeersCount < config.MaxPlayers) req.AcceptIfKey("suffering-1");
    else req.Reject();
};
listener.PeerConnectedEvent += p => Log.Information("Player #{Id} connected from {Addr}", p.Id, p.Address);
listener.PeerDisconnectedEvent += (p, info) => Log.Information("Player #{Id} left ({Reason})", p.Id, info.Reason);
listener.NetworkReceiveEvent += (p, reader, channel, method) =>
{
    var data = reader.GetRemainingBytes();
    reader.Recycle();
};

if (!net.Start(config.Port))
{
    Log.Fatal("Could not bind UDP port {Port}. Is another server already using it?", config.Port);
    await Log.CloseAndFlushAsync();
    return 1;
}
Log.Information("SUFFERING server listening on UDP {Port} (max {Max} players)", config.Port, config.MaxPlayers);

var registry = new CommandRegistry();
registry.RegisterAllFrom(Assembly.GetExecutingAssembly());
var serverCtx = new ServerContext(config, net, registry, cts.Cancel, started);

var pending = new ConcurrentQueue<Action>();
var editor = new LineEditor(() => registry.Names);

_ = Task.Run(() =>
{
    while (!cts.IsCancellationRequested)
    {
        var line = editor.ReadLine(cts.Token);
        if (line is null) return;

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) continue;

        if (registry.TryGet(parts[0], out var cmd))
            pending.Enqueue(() => cmd.Execute(serverCtx, parts[1..]));
        else
            Log.Warning("Unknown command '{Cmd}'. Type help.", parts[0]);
    }
});

var tickMs = 1000.0 / config.TickRate;
using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(tickMs));
try
{
    while (await timer.WaitForNextTickAsync(cts.Token))
    {
        var t0 = Stopwatch.GetTimestamp();

        while (pending.TryDequeue(out var action))
        {
            try { action(); }
            catch (Exception ex) { Log.Error(ex, "Command failed"); }
        }

        net.PollEvents();

        var took = Stopwatch.GetElapsedTime(t0).TotalMilliseconds;
        if (took > tickMs * 2)
            Log.Warning("Can't keep up! Tick took {Took:F0} ms (budget {Budget:F0} ms)", took, tickMs);
    }
}
catch (OperationCanceledException) { }

Log.Information("Stopping server...");
net.DisconnectAll();
net.Stop();
await Log.CloseAndFlushAsync();
return 0;
