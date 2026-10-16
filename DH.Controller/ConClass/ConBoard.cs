namespace DH.Controller.ConClass;

public class ConBoard(ILogger<ConBoard> Logger,
                      IProtocol IProto,
                      CommEntity CommPar,
                      ControlEntity ControlPar,
                      ICommandPublisher ComPublisher) : IConBoard
{
    private Socket HDSocket = default!;

    private const int RemotePort = 2000;

    private const int ReceiveMaxSize = 1400;

    private IPEndPoint LocalPoint = default!;

    private IPEndPoint RemotePoint = default!;

    private CancellationToken ThisCancellationToken;

    public async ValueTask InitHD(CancellationToken cancellationToken)
    {
        ThisCancellationToken = cancellationToken;

        try
        {
            LocalPoint = new(IPAddress.Parse(CommPar.LocalIP), RemotePort + int.Parse(SplitIPSegments(ControlPar.IP, ThisCancellationToken)[3]));

            RemotePoint = new(IPAddress.Parse(ControlPar.IP), RemotePort);

            HDSocket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveBufferSize = ReceiveMaxSize,
                EnableBroadcast = false,
                DontFragment = true,
                ExclusiveAddressUse = true
            };

            HDSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);

            ComPublisher?.CommandFunc += OnCommandEvent;

            if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"初始化 {LocalPoint.Address}:{LocalPoint.Port} 完成.");
        }
        catch (Exception ex)
        {
            if (ThisCancellationToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{LocalPoint.Address}:{LocalPoint.Port}  {ex.Message}");
            }
        }
    }

    public void ShutDown()
    {
        ComPublisher?.CommandFunc -= OnCommandEvent;

        HDSocket?.Dispose();

        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ControlPar?.IP} 已关闭!");
    }

    private ReadOnlySpan<string> SplitIPSegments(in string IPStr, CancellationToken StoppingToken)
    {
        ReadOnlySpan<string> AnaStr = default!;

        try
        {
            AnaStr = IPStr.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).AsSpan();

            if (AnaStr.IsEmpty is true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"拆分IP段为空!");
            }
        }
        catch (Exception ex)
        {
            if (StoppingToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", ex);
            }
        }

        return AnaStr;
    }

    private async Task OnCommandEvent(CommandEventArgs ComType, CancellationToken StopToken)
    {
        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ControlPar?.IP}  指令:{ComType.ComType}  时刻:{ComType.PublishTick}");
    }
}
