namespace DH.Controller.ConClass;

public class ConBoard(ILogger<ConBoard> Logger,
                      IProtocol IProto,
                      ControlEntity ControlPar) : IConBoard
{
    private CancellationToken ThisCancellationToken;

    public void Startup(CancellationToken cancellationToken)
    {
        ThisCancellationToken = cancellationToken;

        try
        {


            if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"初始化 {ControlPar?.IP} 完成.");
        }
        catch (Exception ex)
        {
            if (ThisCancellationToken.IsCancellationRequested is not true)
            {
                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ex.Message}");
            }
        }
    }

    public void ShutDown()
    {
        if (Logger.IsEnabled(LogLevel.Information) is true) Logger.LogInformation("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ControlPar?.IP} 已关闭!");
    }
}
