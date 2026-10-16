namespace DH.Controller;

public interface IConBoard
{
    ValueTask InitHD(CancellationToken cancellationToken);

    void ShutDown();
}
