namespace DH.Entity.ConModule;

/// <summary>
/// dong hui
/// 2026-09-27 12:44
/// 初版
/// 采集板插槽实体
/// </summary>
public record SocketEntity
(
    [property:JsonPropertyName("Socket")]
    int Socket,

    [property:JsonPropertyName("IsEnable")]
    bool IsEnable
)
{
    public SocketEntity() : this(0, false) { }
}
