using DH.Entity;
using DH.Tools;
using System.Reflection;
using DH.Entity.ConModule;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging.Console;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Async;
using System.Text;
using Serilog.Extensions.Hosting;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using DH.Controller;
using DH.Controller.ConClass;
using Microsoft.Extensions.Hosting.Systemd;

namespace Controller
{
    /// <summary>
    /// dong hui
    /// 2026-08-28 11:24
    /// 初版
    /// 程序入口,参数，设置，DI
    /// </summary>
    public class Program
    {
        #region 全局变量

        private static string ConParPath = default!;

        #endregion

        public static async Task Main(string[] args)
        {
            HostApplicationBuilder Builder = Host.CreateApplicationBuilder(args);

            #region 参数设置

            ConParPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"D:{Path.DirectorySeparatorChar}{Path.Combine("tycho", "scratch", "parameterfiles", "con.json")}"
            : $"{Path.DirectorySeparatorChar}{Path.Combine("etc", "scratchpar", "con.json")}";

            IConfigurationBuilder IConfig = Builder
            .Configuration
            .AddJsonFile(ConParPath, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            Builder.Services.Configure<ConEntity>(Builder.Configuration);

            Builder.Services.Configure<HostOptions>(options =>
            {
                options.ShutdownTimeout = TimeSpan.FromSeconds(30);

                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;

                options.ServicesStartConcurrently = true;

                options.ServicesStopConcurrently = true;
            });

            #endregion

            #region 日志设置

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("ProcessId", Environment.ProcessId)
            .Enrich.WithProperty("ThreadId", Environment.CurrentManagedThreadId)
            .Filter.ByExcluding(info => info.MessageTemplate.Text.Contains("心跳包"))
            .WriteTo.Async(ActionAsync =>
            {
                ActionAsync.File
                (
                        shared: false,
                        buffered: false,
                        encoding: Encoding.UTF8,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 365,
                        fileSizeLimitBytes: long.MaxValue,
                        rollingInterval: RollingInterval.Day,
                        retainedFileTimeLimit: TimeSpan.FromDays(365),
                        flushToDiskInterval: TimeSpan.FromMilliseconds(1),
                        path: RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                                              ? $"D:{Path.DirectorySeparatorChar}{Path.Combine("Tycho", "Scratch", "MonitorLog", ".log")}"
                                                              : $"{Path.DirectorySeparatorChar}{Path.Combine("var", "log", "scratch", "monitorlog", ".log")}",
                        formatter: new SpeciaMarkFormatter(new LogTextFormatter(DefaultTemp: "SysTick:{Timestamp:HH:mm:ss.fff}   [{Level:u3}]   [{SourceContext}]   {Message:lj}   {ProcessId}   {ThreadId}   {MachineName}{NewLine}{Exception}",
                                                           SpecialTemp: "{Message:lj}{NewLine}",
                                                           MatchCon: log => log.Properties.ContainsKey("Entry")),
                                                           "Entry")
                        );

                if (Builder.Environment.IsDevelopment())
                {
                    ActionAsync.Console();
                }
            },
            10000,
            false)
            .CreateLogger();

            Builder.Logging.ClearProviders();

            Builder.Services.AddSerilog(logger: null, dispose: true);

            #endregion

            #region 服务注册

            Builder.Services.AddScoped<IController, Contrl>();

            Builder.Services.AddHostedService<ConWorker>();

            Builder.Services.AddWindowsService(options =>
                {
                    options.ServiceName = "ScratchMonitor";
                });

            Builder.Services.AddSystemd();

            #endregion

            #region 主机运行

            IHost HostObj = Builder.Build();

            await HostObj.RunAsync();

            #endregion
        }
    }
}