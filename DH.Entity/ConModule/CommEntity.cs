namespace DH.Entity.ConModule;

/// <summary>
/// dong hui
/// 2026-09-27 12:41
/// 初版
/// 通用参数实体
/// </summary>
public record CommEntity
(
    [property:JsonPropertyName("LocalIP")]
    string LocalIP,

    [property:JsonPropertyName("EncoderAdapter")]
    string EncoderAdapter,

    [property:JsonPropertyName("MaxRetryTimeout")]
    int MaxRetryTimeout,

    [property:JsonPropertyName("ErasureMemory")]
    bool ErasureMemory,

    [property:JsonPropertyName("ExecInterval")]
    int ExecInterval,

    [property:JsonPropertyName("ExecTimeout")]
    int ExecTimeout
)
{
    public CommEntity() : this(string.Empty, string.Empty, 0, false, 0, 0) { }
}
