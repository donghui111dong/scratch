namespace Controller;

/// <summary>
/// dong hui
/// 2026-09-27 12:37
/// 初版
/// 工作控制器
/// </summary>
public class ConWorker(ILogger<ConWorker> Logger,
                       IOptionsMonitor<ConEntity> ConPar,
                       IHostEnvironment Env,
                       IConfiguration Config,
                       IServiceScopeFactory Factory) : BackgroundService
{
    private const int MinTimeintervalChangeParFile = 500;

    private DateTimeOffset LastLogTime = DateTimeOffset.MinValue;

    protected override async Task ExecuteAsync(CancellationToken StoppingToken)
    {
        AssemblyEntity AssemblyObj = default!;

        try
        {
            string AppPathDir = Path.Combine(AppContext.BaseDirectory, "Controller");

            AssemblyObj = JSON.ReadAssemblyInfo<AssemblyEntity>(AppPathDir);

            using (LogContext.PushProperty("Entry", string.Empty))
            {
                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("{Msg}", Config["SpecialSplitChar:SpecialChar"]);

                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("* {Msg}", $"工作环境:{Env?.EnvironmentName}");

                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("* {Msg}", $"工作目录:{AppPathDir}");

                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("* {Msg}", $"工作版本:{AssemblyObj?.Version}");

                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("* {Msg}", $"发布日期:{AssemblyObj?.BuildTime}");

                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("* {Msg}", $"程序作者:{AssemblyObj?.Company}");

                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("{Msg}", Config["SpecialSplitChar:SpecialChar"]);
            }
        }
        catch (Exception e)
        {
            if (!StoppingToken.IsCancellationRequested)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), e);
            }
        }

        AssemblyObj ??= new() { Version = "", Company = "", BuildTime = "" };

        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), "开启参数文件变动监测.");

        ConPar.OnChange(NewCon =>
        {
            if ((DateTimeOffset.Now - LastLogTime).TotalMilliseconds > MinTimeintervalChangeParFile)
            {
                try
                {
                    if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), "监测到参数文件变动!!!");
                }
                catch (Exception e)
                {
                    if (!StoppingToken.IsCancellationRequested)
                    {
                        if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), e);
                    }
                }

                LastLogTime = DateTimeOffset.Now;
            }
        });

        while (!StoppingToken.IsCancellationRequested)
        {
            try
            {
                if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), "主机已运行.");

                IController IC = Factory.CreateScope().ServiceProvider.GetRequiredService<IController>();

                await IC.Startup(AssemblyObj, StoppingToken);

                await IC.Shutdown(StoppingToken);

                await Task.Delay(1000 * 10, StoppingToken);
            }
            catch (Exception e)
            {
                if (!StoppingToken.IsCancellationRequested)
                {
                    if (Logger.IsEnabled(LogLevel.Warning) is true) Logger.LogWarning("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), e);
                }
            }
        }

        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), "主机已停止!");
    }
}
