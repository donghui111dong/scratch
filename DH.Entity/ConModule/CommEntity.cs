namespace DH.Entity.ConModule;

/// <summary>
/// dong hui
/// 2026-09-27 12:41
/// 初版
/// 通用参数实体
/// </summary>
public class CommEntity
{
    public string LocalIP { get; set; } = default!;

    public string EncoderAdapter { get; set; } = default!;

    public int MaxRetryTimeout { get; set; }

    public bool ErasureMemory { get; set; }

    public int ExecInterval { get; set; }

    public int ExecTimeout { get; set; }
}
