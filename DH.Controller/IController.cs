namespace DH.Controller;

/// <summary>
/// dong hui
/// 2026-09-27 12:39
/// 初版
/// 主控制器接口
/// </summary>
public interface IController
{
    Task Startup(AssemblyEntity AssemblyObj, CancellationToken StoppingToken);

    Task Shutdown(CancellationToken StoppingToken);
}
