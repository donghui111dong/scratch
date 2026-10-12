namespace DH.Entity.ConModule;

/// <summary>
/// dong hui
/// 2026-09-27 12:42
/// 初版
/// 控制参数实体
/// </summary>
public record ConEntity
(
    [property:JsonPropertyName("Comm")]
    CommEntity Comm,

    [property:JsonPropertyName("Control")]
    ImmutableList<ControlEntity> Control
)
{
    public ConEntity() : this(default!, []) { }
}
