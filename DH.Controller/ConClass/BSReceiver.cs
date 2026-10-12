namespace DH.Controller.ConClass;

public class BSReceiver(ILogger<Contrl> Logger,
                        IOptionsMonitor<ConEntity> ConPar,
                        ChannelWriter<PacketBytesForBsr> ChnWriter)
{
    private Socket? SocketObj;

    private EndPoint? LocalPointObj;

    private const int ReceiveBufferLength = 512;

    private CancellationTokenSource? LinkTokenSource;

    private readonly CancellationTokenSource InnerToken = new();

    private readonly MemoryPool<byte> BufferPool = MemoryPool<byte>.Shared;

    private readonly EndPoint RemotePointObj = new IPEndPoint(IPAddress.Any, 0);

    private readonly Channel<ReadOnlyMemory<byte>> DataChannel = Channel.CreateBounded<ReadOnlyMemory<byte>>
    (
        new BoundedChannelOptions(4)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        }
    );

    public async ValueTask Startup(CancellationToken ExternalToken)
    {
        LinkTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ExternalToken, InnerToken.Token);

        LocalPointObj = new IPEndPoint(IPAddress.Any, ConPar.CurrentValue.Comm.BSPort);

        SocketObj = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            ReceiveBufferSize = ReceiveBufferLength,
            EnableBroadcast = true,
            ExclusiveAddressUse = false
        };

        SocketObj.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        SocketObj.Bind(LocalPointObj);

        ValueTask RLTask = ReceviceLoop(LinkTokenSource.Token);

        ValueTask RKTask = ValueTask.CompletedTask;//ReceviceLoop(LinkTokenSource.Token);

        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"信号接收已启动，监听地址：{(LocalPointObj as IPEndPoint)!.Address}:{(LocalPointObj as IPEndPoint)!.Port}");

        await Task.WhenAll(RLTask.AsTask(), RKTask.AsTask());
    }

    public void Shutdown()
    {
        try

        {
            LinkTokenSource?.Cancel();

            LinkTokenSource?.Dispose();

            InnerToken?.Cancel();

            InnerToken?.Dispose();

            SocketObj?.Dispose();
        }
        catch (Exception e)
        {
            e.Message.ToString();
        }

        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"信号接收已停止!");
    }

    private async ValueTask ReceviceLoop(CancellationToken StopCancellationToken)
    {
        IMemoryOwner<byte> CurrentReceiveBytes;

        bool StopReGCForThisRegionBlock = GC.TryStartNoGCRegion(ReceiveBufferLength * 2);

        try
        {
            while (StopCancellationToken.IsCancellationRequested is not true)
            {
                CurrentReceiveBytes = BufferPool.Rent(ReceiveBufferLength);

                SocketReceiveMessageFromResult Message = await SocketObj!.ReceiveMessageFromAsync(CurrentReceiveBytes.Memory, SocketFlags.None, RemotePointObj, StopCancellationToken);

                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"接收到元数据->网口序号:{Message.PacketInformation.Interface} 信息来源:{Message.RemoteEndPoint} 数据长度:{Message.ReceivedBytes}");

                try
                {
                    await ChnWriter.WriteAsync(new PacketBytesForBsr() { MemoryOwner = CurrentReceiveBytes, Message = Message }, StopCancellationToken);
                }
                catch (Exception ex)
                {
                    ex.Message.ToString();
                }
            }
        }
        catch (Exception e)
        {
            if (StopCancellationToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{e.Message}");
            }
        }
        finally
        {
            if (StopReGCForThisRegionBlock is true)
            {
                if (StopReGCForThisRegionBlock is true)
                {
                    try
                    {
                        GC.EndNoGCRegion();
                    }
                    catch (InvalidOperationException exx)
                    {
                        exx.Message.ToString();
                    }
                }
            }

            ChnWriter?.TryComplete();
        }
    }
}
