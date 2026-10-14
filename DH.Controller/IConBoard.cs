namespace DH.Controller;

public interface IConBoard
{
    void Startup(CancellationToken cancellationToken);

    void ShutDown();
}
