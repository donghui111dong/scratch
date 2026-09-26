namespace DH.Controller;

public interface IController
{
    Task Startup(AssemblyEntity AssemblyObj, CancellationToken StoppingToken);

    Task Shutdown(CancellationToken StoppingToken);
}
