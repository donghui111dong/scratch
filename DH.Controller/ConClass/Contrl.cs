using System.Net;

/// <summary>
/// dong hui
/// 2026-09-27 12:39
/// 初版
/// 主控制器
/// </summary>
namespace DH.Controller.ConClass;

public class Contrl(ILogger<Contrl> Logger,
                    IOptionsMonitor<ConEntity> ConPar) : IController
{
    public async Task Startup(AssemblyEntity AssemblyObj, CancellationToken StoppingToken)
    {
        if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", "主控制器已启动.");

        bool PingStatus = await CheckNetworkStatus.GetNetStatus(ConPar.CurrentValue.Comm.LocalIP, Logger, StoppingToken);

        if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", $"网络状态检测正常: {ConPar.CurrentValue.Comm.LocalIP} {PingStatus}");

        while (!StoppingToken.IsCancellationRequested)
        {
            try
            {
                if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", $"主控制器运行中...");

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
        await Task.CompletedTask;
    }
}

file class CheckNetworkStatus()
{
    public static async ValueTask<bool> GetNetStatus(string LocalIP, ILogger<Contrl> Logger, CancellationToken StoppingToken)
    {
        bool NetStatus = false;

        bool RecordLoginfo = true;

        Socket SocketObj = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint LocalPoint = new(IPAddress.Parse(LocalIP), 0);

        while (!StoppingToken.IsCancellationRequested)
        {
            try
            {
                SocketObj?.Bind(LocalPoint);

                NetStatus = SocketObj is not null;

                SocketObj?.Close();

                SocketObj?.Dispose();
            }
            catch (Exception ex)
            {
                NetStatus = false;

                ex.Message.ToString();
            }

            if (NetStatus is true)
            {
                break;
            }
            else
            {
                if (RecordLoginfo is true)
                {
                    if (Logger.IsEnabled(LogLevel.Error)) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", $"检测到网络状态异常: {LocalIP} {NetStatus}");

                    RecordLoginfo = false;
                }

                await Task.Delay(2000, StoppingToken);
            }
        }

        return NetStatus;
    }
}

file class NetworkStatusObserver()
{

}
