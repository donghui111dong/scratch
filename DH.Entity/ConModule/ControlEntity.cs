namespace DH.Entity.ConModule;

/// <summary>
/// dong hui
/// 2026-09-27 12:42
/// 初版
/// 硬件参数实体
/// </summary>
public class ControlEntity
{
    public string IP { get; set; } = default!;

    public bool IsEnable { get; set; }

    public List<SocketEntity> Sockets { get; set; } = default!;
}
