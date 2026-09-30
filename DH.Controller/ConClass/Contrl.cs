namespace DH.Controller.ConClass;

/// <summary>
/// dong hui
/// 2026-09-27 12:39
/// 初版
/// 主控制器
/// </summary>
public class Contrl(ILogger<Contrl> Logger,
                    IConfiguration Config,
                    IOptionsMonitor<ConEntity> ConPar) : IController
{
    public async Task Startup(AssemblyEntity AssemblyObj, CancellationToken StoppingToken)
    {
        if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", "主控制器已启动.");

        bool PingStatus = await CheckNetworkStatus.GetNetStatus(ConPar.CurrentValue.Comm.LocalIP, Logger, StoppingToken);

        if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", $"网络状态检测正常: {ConPar.CurrentValue.Comm.LocalIP} {PingStatus}");

        NetworkStatusObserver.Startup(Logger, Config, ConPar, StoppingToken);

        while (!StoppingToken.IsCancellationRequested)
        {
            try
            {
                //if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", $"{ConPar.CurrentValue.Comm.ToString()}");

                await Task.Delay(500, StoppingToken);
            }
            catch (Exception e)
            {
                if (!StoppingToken.IsCancellationRequested)
                {
                    if (Logger.IsEnabled(LogLevel.Error)) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", e);
                }
            }
        }

        if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", "主控制器已停止!!!");
    }

    public async Task Shutdown(CancellationToken StoppingToken)
    {
        NetworkStatusObserver.Shutdown(Logger, StoppingToken);

        await Task.CompletedTask;
    }
}

file class CheckNetworkStatus()
{
    public static async ValueTask<bool> GetNetStatus(string LocalIP, ILogger<Contrl> Logger, CancellationToken StoppingToken)
    {
        bool NetStatus = false;

        bool RecordLoginfo = true;

        Ping NetPingChecker = new();

        PingReply PingCheckerResult = default!;

        while (!StoppingToken.IsCancellationRequested)
        {
            try
            {
                PingCheckerResult = await NetPingChecker.SendPingAsync(IPAddress.Parse(LocalIP), TimeSpan.FromMilliseconds(2000), null, null, StoppingToken);

                NetStatus = PingCheckerResult.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                if (StoppingToken.IsCancellationRequested is not true)
                {
                    NetStatus = false;

                    ex.Message.ToString();
                }
            }

            if (NetStatus is true)
            {
                NetPingChecker?.Dispose();

                break;
            }
            else
            {
                if (RecordLoginfo is true)
                {
                    if (Logger.IsEnabled(LogLevel.Error)) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", $"网络异常终止执行!  {LocalIP}  {PingCheckerResult.Status}");

                    RecordLoginfo = false;
                }

                await Task.Delay(1000, StoppingToken);
            }
        }

        return NetStatus;
    }
}

file class NetworkStatusObserver()
{
    private const int MaxTimeTickNetChange = 1000;

    private static IConfiguration NetConfig = default!;

    private static ILogger<Contrl> NetLogger = default!;

    private static IOptionsMonitor<ConEntity> NetPar = default!;

    private static CancellationToken NetStoppingToken = default!;

    private static DateTimeOffset LastTimeTickIPChange = DateTimeOffset.MinValue;

    private static DateTimeOffset LastTimeTickKeepChange = DateTimeOffset.MinValue;

    private static readonly Channel<NetStatusEvent> NetChannel = Channel.CreateBounded<NetStatusEvent>
    (
        new BoundedChannelOptions(10)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        }
    );

    private static readonly ObjectPool<StringBuilder> StrPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();

    private static readonly NetworkAddressChangedEventHandler? NwAddressHandler = async (sender, e) =>
    {
        if ((DateTimeOffset.Now - LastTimeTickIPChange).TotalMilliseconds > MaxTimeTickNetChange)
        {
            try
            {
                await NetChannel.Writer.WriteAsync(new NetStatusEvent(NetStatusType.IPChanged), NetStoppingToken);
            }
            catch (Exception ex)
            {
                if (NetStoppingToken.IsCancellationRequested is not true)
                {
                    if (NetLogger.IsEnabled(LogLevel.Error)) NetLogger.LogError("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", ex);
                }
            }

            LastTimeTickIPChange = DateTimeOffset.Now;
        }
    };

    private static readonly NetworkAvailabilityChangedEventHandler? NwAvaliHander = async (sender, e) =>
    {
        if ((DateTimeOffset.Now - LastTimeTickKeepChange).TotalMilliseconds > MaxTimeTickNetChange)
        {
            try
            {
                await NetChannel.Writer.WriteAsync(new NetStatusEvent(NetStatusType.NetKeepChanged, e.IsAvailable), NetStoppingToken);
            }
            catch (Exception ex)
            {
                if (NetStoppingToken.IsCancellationRequested is not true)
                {
                    if (NetLogger.IsEnabled(LogLevel.Error)) NetLogger.LogError("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", ex);
                }
            }

            LastTimeTickKeepChange = DateTimeOffset.Now;
        }
    };

    private static bool? CheckSpecifiedIPExists(IEnumerable<UnicastIPAddressInformation> NetRecord, string IP)
    {
        return NetRecord?.Any(nic => nic.Address.Equals(IP));
    }

    private static IEnumerable<UnicastIPAddressInformation> GetAllNetworkInterface()
    {
        return NetworkInterface.GetAllNetworkInterfaces()

                .Where(nic => nic.OperationalStatus == OperationalStatus.Up

                && nic.NetworkInterfaceType is not NetworkInterfaceType.Loopback

                and not NetworkInterfaceType.Tunnel

                and not NetworkInterfaceType.Unknown

                && !nic.Description.Contains("vEthernet", StringComparison.OrdinalIgnoreCase)

                && !nic.Description.Contains("VMware", StringComparison.OrdinalIgnoreCase)

                && !nic.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase)

                && !nic.Description.Contains("VMware", StringComparison.OrdinalIgnoreCase)

                && !nic.Description.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase)

                && !nic.Description.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase))

                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)

                .Where(nic => nic.Address.AddressFamily == AddressFamily.InterNetwork);
    }

    private static async Task ReadChannelInfo()
    {
        try
        {
            while (await NetChannel.Reader.WaitToReadAsync(NetStoppingToken))
            {
                if (NetChannel.Reader.TryRead(out NetStatusEvent? NTInfo) is true && NTInfo is not null)
                {
                    using (LogContext.PushProperty("Entry", string.Empty))
                    {
                        if (NetLogger.IsEnabled(LogLevel.Warning)) NetLogger.LogWarning("{Msg}", $"{NetConfig["SpecialSplitChar:SpecialChar"]}");

                        if (NetLogger.IsEnabled(LogLevel.Warning)) NetLogger.LogWarning("*  触发时刻:{Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}");

                        if (NetLogger.IsEnabled(LogLevel.Warning)) NetLogger.LogWarning("*  {Msg}",
                                        $@"监测到{NTInfo?.ChangeType switch
                                        {
                                            NetStatusType.IPChanged => " IP信息 ",

                                            NetStatusType.NetKeepChanged => " 网口状态 ",

                                            _ => "未知"
                                        }}有变动");

                        IEnumerable<UnicastIPAddressInformation> AllNetRecord = GetAllNetworkInterface();

                        StringBuilder Str = StrPool.Get();

                        Str.Clear();

                        AllNetRecord?.ToList().ForEach(nic => { Str.AppendLine($"*  {nic.Address}  {nic.IPv4Mask}"); });

                        if (NetLogger.IsEnabled(LogLevel.Warning)) NetLogger.LogWarning("{Msg}", $"{Str}");

                        Str.Clear();

                        StrPool.Return(Str);

                        if (NTInfo?.ChangeType == NetStatusType.NetKeepChanged)
                        {
                            if (NetLogger.IsEnabled(LogLevel.Warning)) NetLogger.LogWarning("*  {Msg}", $"网口状态: {NTInfo?.NetKeep}");
                        }

                        if (CheckSpecifiedIPExists(AllNetRecord = default!, NetPar.CurrentValue.Comm.LocalIP) is not true)
                        {
                            if (NetLogger.IsEnabled(LogLevel.Warning)) NetLogger.LogWarning("*  {Msg}", $"指定要用的IP不存在: {NetPar.CurrentValue.Comm.LocalIP}");
                        }

                        if (NetLogger.IsEnabled(LogLevel.Information) is true) NetLogger.LogInformation("{Msg}", NetConfig["SpecialSplitChar:SpecialChar"]);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (NetStoppingToken.IsCancellationRequested is not true)
            {
                if (NetLogger.IsEnabled(LogLevel.Error)) NetLogger.LogError("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", ex);
            }
        }
    }

    public static void Startup(ILogger<Contrl> Logger, IConfiguration Config, IOptionsMonitor<ConEntity> ConPar, CancellationToken StoppingToken)
    {
        NetPar = ConPar;

        NetLogger = Logger;

        NetConfig = Config;

        NetStoppingToken = StoppingToken;

        try
        {
            NetworkChange.NetworkAddressChanged += NwAddressHandler;

            NetworkChange.NetworkAvailabilityChanged += NwAvaliHander;

            _ = ReadChannelInfo();
        }
        catch (Exception ex)
        {
            if (StoppingToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error)) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", ex);
            }
        }
    }

    public static void Shutdown(ILogger<Contrl> Logger, CancellationToken StoppingToken)
    {
        try
        {
            NetworkChange.NetworkAddressChanged -= NwAddressHandler;

            NetworkChange.NetworkAvailabilityChanged -= NwAvaliHander;

            NetChannel?.Writer?.TryComplete();
        }
        catch (Exception ex)
        {
            if (StoppingToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error)) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", ex);
            }
        }
    }
}
