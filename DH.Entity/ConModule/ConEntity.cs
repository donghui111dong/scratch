namespace DH.Entity.ConModule;

/// <summary>
/// dong hui
/// 2026-09-27 12:42
/// 初版
/// 控制参数实体
/// </summary>
public class ConEntity
{
    public CommEntity Comm { get; set; } = default!;

    public List<ControlEntity> Control { get; set; } = default!;
}
