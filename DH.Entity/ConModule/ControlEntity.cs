namespace DH.Entity.ConModule;

public class ControlEntity
{
    public string IP { get; set; } = default!;

    public bool IsEnable { get; set; }

    public List<SocketEntity> Sockets { get; set; } = default!;
}
