namespace DH.Events.Command;

public class CommandEventArgs(CommandType InComType) : EventArgs
{
    public CommandType ComType { get; } = InComType;

    public string PublishTick { get => $"{DateTime.Now:HH:mm:ss.fff}"; }
}
