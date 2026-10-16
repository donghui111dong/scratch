namespace DH.Events.Command;

public interface ICommandPublisher
{
    event Func<CommandEventArgs, CancellationToken, Task> CommandFunc;

    Task CommandAsync(CommandEventArgs ComArgs, CancellationToken StopToken);
}
