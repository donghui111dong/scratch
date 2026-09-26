namespace DH.Controller.ConClass;

public class Contrl(ILogger<Contrl> Logger) : IController
{
    public async Task Startup(AssemblyEntity AssemblyObj, CancellationToken StoppingToken)
    {
        if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", "主控制器已启动.");

        while (!StoppingToken.IsCancellationRequested)
        {
            try
            {
                if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", "控制器运行中...");

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
    private void GetNetStatus()
    {

    }
}
