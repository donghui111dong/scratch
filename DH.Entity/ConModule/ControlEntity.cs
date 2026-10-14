namespace DH.Entity.ConModule;

/// <summary>
/// dong hui
/// 2026-09-27 12:42
/// 初版
/// 硬件参数实体
/// </summary>
public record ControlEntity
(
    [property:JsonPropertyName("IP")]
    string IP,

    [property:JsonPropertyName("IsEnable")]
    bool IsEnable,

    [property:JsonPropertyName("Sockets")]
    List<SocketEntity> Sockets
)
{
    public ControlEntity() : this(string.Empty, false, []) { }
}
