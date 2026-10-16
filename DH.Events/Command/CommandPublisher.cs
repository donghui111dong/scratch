namespace DH.Events.Command;

public class CommandPublisher(ILogger<CommandPublisher> Logger) : ICommandPublisher
{
    private event Func<CommandEventArgs, CancellationToken, Task> CommandProducedFunc = default!;

    public event Func<CommandEventArgs, CancellationToken, Task> CommandFunc
    {
        add => CommandProducedFunc += value;

        remove => CommandProducedFunc -= value;
    }

    public async Task CommandAsync(CommandEventArgs ComArgs, CancellationToken StopToken)
    {
        if (ComArgs is not null)
        {
            List<Task> ExecList = [];

            List<Func<CommandEventArgs, CancellationToken, Task>> DelegeteList = [.. CommandProducedFunc.GetInvocationList().Cast<Func<CommandEventArgs, CancellationToken, Task>>()];

            if (DelegeteList is not null && DelegeteList.Count > 0)
            {
                try
                {
                    await Parallel.ForEachAsync(DelegeteList, new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount,
                        CancellationToken = StopToken
                    },
                    async (AsyncObj, TokenObj) =>
                    {
                        try
                        {
                            ExecList.Add(AsyncObj(ComArgs, TokenObj));
                        }
                        catch (Exception ex)
                        {
                            if (StopToken.IsCancellationRequested is not true)
                            {
                                if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ex.Message}");
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    if (StopToken.IsCancellationRequested is not true)
                    {
                        if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ex.Message}");
                    }
                }

                try
                {
                    await Task.WhenAll(ExecList);
                }
                catch (Exception ex)
                {
                    if (StopToken.IsCancellationRequested is not true)
                    {
                        if (Logger.IsEnabled(LogLevel.Error) is true) Logger.LogError("LogTick:{Tick}   {Msg}", $"{DateTimeOffset.Now:HH:mm:ss.fff}", $"{ex.Message}");
                    }
                }
            }
        }
    }
}
