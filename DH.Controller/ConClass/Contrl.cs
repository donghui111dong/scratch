namespace DH.Controller.ConClass;

/// <summary>
/// dong hui
/// 2026-09-27 12:39
/// 初版
/// 主控制器
/// </summary>
public class Contrl(ILogger<Contrl> Logger,
                    IConfiguration Config,
                    IProtocol IProto,
                    BSReceiver BSrec,
                    IConBoardFactory ICBoardFactory,
                    IOptionsMonitor<ConEntity> ConPar,
                    ICommandPublisher ComPublisher,
                    [FromKeyedServices("BSRReader")] ChannelReader<PacketBytesForBsr> ChaReader
                    ) : IController
{
    private string LastSignal = "0";

    private readonly List<IConBoard> IConBoardList = [];

    private readonly ArrayPool<byte> PacketPool = ArrayPool<byte>.Shared;

    public async Task Startup(AssemblyEntity AssemblyObj, CancellationToken StoppingToken)
    {
        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", "主控制器已启动.");

        bool PingStatus = await CheckNetworkStatus.GetNetStatus(ConPar.CurrentValue.Comm.LocalIP, Logger, StoppingToken).ConfigureAwait(false);

        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"网络状态检测正常: {ConPar.CurrentValue.Comm.LocalIP} {PingStatus}");

        NetworkStatusObserver.Startup(Logger, Config, ConPar, StoppingToken);

        ValueTask BSTask = BSrec.Startup(StoppingToken);

        try
        {
            await BuildConHD(StoppingToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        catch (Exception ex)
        {
            if (StoppingToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ex.Message}");
            }
        }

        try
        {
            while (StoppingToken.IsCancellationRequested is not true && await ChaReader.WaitToReadAsync(StoppingToken).ConfigureAwait(false))
            {
                try
                {
                    if (ChaReader.TryRead(out PacketBytesForBsr? PacketBytes) is true && PacketBytes is not null)
                    {
                        ReadOnlySpan<string> BsrInfo = AnalysisBsrInfo(in PacketBytes, StoppingToken);

                        if (BsrInfo.IsEmpty is not true)
                        {
                            _ = BsrInfo[2] switch
                            {
                                "1" => LastSignal is not "1" ? SignalAction(BsrInfo[2], StoppingToken) : default,
                                "0" => LastSignal is not "0" ? SignalAction(BsrInfo[2], StoppingToken) : default,
                                _ => default
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (StoppingToken.IsCancellationRequested is not true)
                    {
                        if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ex.Message}");
                    }
                }
            }
        }
        catch (Exception exx)
        {
            if (StoppingToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{exx.Message}");
            }
        }

        try
        {
            await BSTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException oce)
        {
            oce.Message.ToString();
        }

        try
        {
            //await Task.WhenAll(HDRunTask);
        }
        catch (Exception ex)
        {
            ex.Message.ToString();
        }
    }

    public async Task Shutdown(CancellationToken StoppingToken)
    {
        NetworkStatusObserver.Shutdown(Logger, StoppingToken);

        BSrec?.Shutdown();

        foreach (IConBoard item in IConBoardList)
        {
            try
            {
                item?.ShutDown();
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        IConBoardList?.Clear();

        try
        {
            await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.None);
        }
        catch (OperationCanceledException oce)
        {
            oce.Message.ToString();
        }

        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", "主控制器已停止!");
    }

    private ReadOnlySpan<string> AnalysisBsrInfo(in PacketBytesForBsr BsrPacketBytes, CancellationToken StoppingToken)
    {
        Span<string> StrList = default!;

        try
        {
            ReadOnlySpan<byte> MemorySpan = BsrPacketBytes.MemoryOwner.Memory.Span[..BsrPacketBytes.Message.ReceivedBytes];

            char[] CharPool = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetCharCount(MemorySpan));

            Encoding.UTF8.GetChars(MemorySpan, CharPool.AsSpan()[..MemorySpan.Length]);

            string AnaStr = new(CharPool.AsSpan()[..MemorySpan.Length]);

            if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"网口原始信号数据:{AnaStr}");

            if (AnaStr.Contains("BEGIN", StringComparison.OrdinalIgnoreCase) is true && AnaStr.Contains("END", StringComparison.OrdinalIgnoreCase) is true)
            {
                StrList = AnaStr.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).AsSpan();

                StrList[1] = StrList[1][5..];

                StrList[2] = StrList[2][7..];

                if (StrList.Length >= 5)
                {
                    StrList[3] = StrList[3][4..];
                }
            }
            else
            {
                if (Logger.IsEnabled(LogLevel.Warning) is true) Logger.LogWarning("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"接收到未知原始信号数据,已丢弃!");
            }
        }
        catch (Exception ex)
        {
            if (StoppingToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", ex);
            }
        }
        finally
        {
            BsrPacketBytes?.MemoryOwner?.Dispose();
        }

        return StrList;
    }

    private async Task<string> SignalAction(string CurrentSignal, CancellationToken StoppingToken)
    {
        LastSignal = CurrentSignal;

        List<Task> ExecCommandTaskList = [];

        ReadOnlyMemory<CommandType> CTypeMem = new([
                                                        CommandType.StartHD,
                                                        CommandType.WorkStatus,
                                                        CommandType.StopHD,
                                                        CommandType.WorkStatus,
                                                        CommandType.ReadData,
                                                        CommandType.ExceOver
                                                    ]);

        if (CurrentSignal == "1")
        {
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    ExecCommandTaskList.Add(ComPublisher.CommandAsync(new(CTypeMem.Span[i]), StoppingToken));

                    if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"已写入指令用通道 -> command:{CTypeMem.Span[i]}");
                }
                catch (Exception ex)
                {
                    if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ex.Message}");
                }
            }
        }
        else if (CurrentSignal == "0")
        {
            for (int i = 2; i < CTypeMem.Length; i++)
            {
                ExecCommandTaskList.Add(ComPublisher.CommandAsync(new(CTypeMem.Span[i]), StoppingToken));

                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"已写入指令用通道 -> command:{CTypeMem.Span[i]}");
            }
        }

        try
        {
            await Task.WhenAll(ExecCommandTaskList).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        catch (Exception ex)
        {
            if (StoppingToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Warning) is true) Logger.LogWarning("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ex.Message}");
            }
        }

        return default!;
    }

    private async Task BuildConHD(CancellationToken StoppingToken)
    {
        if (ConPar.CurrentValue is not null && ConPar.CurrentValue.Control is not null && ConPar.CurrentValue.Control.Count > 0)
        {
            try
            {
                await Parallel.ForEachAsync(ConPar.CurrentValue.Control,
                                            new ParallelOptions()
                                            {
                                                MaxDegreeOfParallelism = ConPar.CurrentValue.Control.Count,
                                                CancellationToken = StoppingToken
                                            },
                                            async (Control, StoppingToken) =>
                                            {
                                                try
                                                {
                                                    if (Control.IsEnable)
                                                    {
                                                        IConBoard ICB = ICBoardFactory.CreateIConBoard(ConPar.CurrentValue.Comm, Control);

                                                        try
                                                        {
                                                            await ICB.InitHD(StoppingToken).ConfigureAwait(false);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            if (StoppingToken.IsCancellationRequested is not true)
                                                            {
                                                                if (Logger.IsEnabled(LogLevel.Warning) is true) Logger.LogWarning("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ex.Message}");
                                                            }
                                                        }

                                                        IConBoardList.Add(ICB!);
                                                    }
                                                    else
                                                    {
                                                        if (StoppingToken.IsCancellationRequested is not true)
                                                        {
                                                            if (Logger.IsEnabled(LogLevel.Warning) is true) Logger.LogWarning("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{Control.IP} 被禁用!");
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    if (StoppingToken.IsCancellationRequested is not true)
                                                    {
                                                        if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", ex);
                                                    }
                                                }
                                            }
                                      ).ConfigureAwait(ConfigureAwaitOptions.None);
            }
            catch (Exception ex)
            {
                if (StoppingToken.IsCancellationRequested is not true)
                {
                    if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", ex);
                }
            }
        }
        else
        {
            if (StoppingToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", "控制参数为空或异常,无法初始化采集板控制对象!");
            }
        }
    }
}

// <summary>
/// dong hui
/// 2026-10-01 00:13
/// 初版
/// 网络检测
/// </summary>
file class CheckNetworkStatus()
{
    public static async ValueTask<bool> GetNetStatus(string LocalIP, ILogger<Contrl> Logger, CancellationToken StoppingToken)
    {
        bool NetStatus = false;

        bool RecordLoginfo = true;

        Ping NetPingChecker = new();

        PingReply PingCheckerResult = default!;

        while (StoppingToken.IsCancellationRequested is not true)
        {
            try
            {
                PingCheckerResult = await NetPingChecker.SendPingAsync(IPAddress.Parse(LocalIP), TimeSpan.FromMilliseconds(2000), null, null, StoppingToken).ConfigureAwait(ConfigureAwaitOptions.None);

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
                    if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"网络异常终止执行!  {LocalIP}  {PingCheckerResult.Status}");

                    RecordLoginfo = false;
                }

                await Task.Delay(1000, StoppingToken).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }

        return NetStatus;
    }
}

// <summary>
/// dong hui
/// 2026-10-01 00:13
/// 初版
/// 网络监视
/// </summary>
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
                await NetChannel.Writer.WriteAsync(new NetStatusEvent(NetStatusType.IPChanged), NetStoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (NetStoppingToken.IsCancellationRequested is not true)
                {
                    if (NetLogger.IsEnabled(LogLevel.Error) is true) NetLogger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", ex);
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
                await NetChannel.Writer.WriteAsync(new NetStatusEvent(NetStatusType.NetKeepChanged, e.IsAvailable), NetStoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (NetStoppingToken.IsCancellationRequested is not true)
                {
                    if (NetLogger.IsEnabled(LogLevel.Error) is true) NetLogger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", ex);
                }
            }

            LastTimeTickKeepChange = DateTimeOffset.Now;
        }
    };

    private static bool? CheckSpecifiedIPExists(IEnumerable<UnicastIPAddressInformation>? NetRecord, string IP)
    {
        return NetRecord?.Any(nic => nic.Address.ToString().Trim() == IP.Trim());
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
            while (await NetChannel.Reader.WaitToReadAsync(NetStoppingToken).ConfigureAwait(false))
            {
                if (NetChannel.Reader.TryRead(out NetStatusEvent? NTInfo) is true && NTInfo is not null)
                {
                    using (LogContext.PushProperty("Entry", string.Empty))
                    {
                        if (NetLogger.IsEnabled(LogLevel.Warning) is true) NetLogger.LogWarning("{Msg}", $"{NetConfig["SpecialSplitChar:SpecialChar"]}");

                        if (NetLogger.IsEnabled(LogLevel.Warning) is true) NetLogger.LogWarning("*  触发时刻:{Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}");

                        if (NetLogger.IsEnabled(LogLevel.Warning) is true) NetLogger.LogWarning("*  {Msg}",
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

                        if (NetLogger.IsEnabled(LogLevel.Warning) is true) NetLogger.LogWarning("{Msg}", $"{Str}");

                        Str.Clear();

                        StrPool.Return(Str);

                        if (NTInfo?.ChangeType == NetStatusType.NetKeepChanged)
                        {
                            if (NetLogger.IsEnabled(LogLevel.Warning) is true) NetLogger.LogWarning("*  {Msg}", $"网口状态: {NTInfo?.NetKeep}");
                        }

                        if (CheckSpecifiedIPExists(AllNetRecord, NetPar.CurrentValue.Comm.LocalIP) is not true)
                        {
                            if (NetLogger.IsEnabled(LogLevel.Warning) is true) NetLogger.LogWarning("*  {Msg}", $"指定要用的IP不存在: {NetPar.CurrentValue.Comm.LocalIP}");
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
                if (NetLogger.IsEnabled(LogLevel.Error) is true) NetLogger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", ex);
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

            if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", "网络监视已启动.");
        }
        catch (Exception ex)
        {
            if (StoppingToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", ex);
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
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", ex);
            }
        }

        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", "网络监视已停止!");
    }
}
