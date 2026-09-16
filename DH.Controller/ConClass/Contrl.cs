using System.Reflection;
using DH.Entity;
using DH.Entity.ConModule;
using DH.Tools;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DH.Controller;

namespace DH.Controller.ConClass;

public class Contrl(ILogger<Contrl> Logger) : IController
{
    public async Task Startup(AssemblyEntity AssemblyObj, CancellationToken StoppingToken)
    {
        Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", "主控制器已启动.");

        while (!StoppingToken.IsCancellationRequested)
        {
            try
            {
                Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", "控制器运行中...");

                await Task.Delay(1000, StoppingToken);
            }
            catch (Exception e)
            {
                if (!StoppingToken.IsCancellationRequested)
                {
                    Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", e);
                }
            }
        }

        Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTime.Now:HH:mm:ss.fff}", "主控制器已停止!");
    }
}
