using System.Reflection;
using DH.Entity;
using DH.Entity.ConModule;
using DH.Tools;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Microsoft.Extensions.Configuration;
using DH.Controller.ConClass;
using DH.Controller;
using System.Linq.Expressions;

namespace Controller
{
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
                    Logger.LogInformation("{Msg}", Config["SpecialSplitChar:SpecialChar"]);

                    Logger.LogInformation("* {Msg}", $"工作环境:{Env?.EnvironmentName}");

                    Logger.LogInformation("* {Msg}", $"工作目录:{AppPathDir}");

                    Logger.LogInformation("* {Msg}", $"工作版本:{AssemblyObj?.Version}");

                    Logger.LogInformation("* {Msg}", $"发布日期:{AssemblyObj?.BuildTime}");

                    Logger.LogInformation("* {Msg}", $"程序作者:{AssemblyObj?.Company}");

                    Logger.LogInformation("{Msg}", Config["SpecialSplitChar:SpecialChar"]);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), e);
            }

            AssemblyObj ??= new() { Version = "", Company = "", BuildTime = "" };

            ConPar.OnChange(NewCon =>
            {
                if ((DateTimeOffset.Now - LastLogTime).TotalMilliseconds > MinTimeintervalChangeParFile)
                {
                    try
                    {
                        Logger.LogInformation("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), "记录到参数变动!!!");
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), e);
                    }

                    LastLogTime = DateTimeOffset.Now;
                }
            });

            while (!StoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Factory.CreateScope().ServiceProvider.GetRequiredService<IController>().Startup(AssemblyObj, StoppingToken);

                    await Task.Delay(1000 * 10, StoppingToken);
                }
                catch (Exception e)
                {
                    if (!StoppingToken.IsCancellationRequested)
                    {
                        Logger.LogWarning("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), e);
                    }
                }
            }

            Logger.LogInformation("LogTick:{Tick}   {Msg}", DateTime.Now.ToString("HH:mm:ss.fff"), "主机已停止!");
        }
    }
}
