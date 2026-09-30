namespace DH.Entity;

/// <summary>
/// dong hui
/// 2026-09-27 12:40
/// 初版
/// 版本信息实体
/// </summary>
public record AssemblyEntity
(
    [property:JsonPropertyName("Version")]
    string Version,

    [property:JsonPropertyName("BuildTime")]
    string BuildTime,

    [property:JsonPropertyName("Company")]
    string Company,

    [property:JsonPropertyName("FrameVersion")]
    string FrameVersion
)
{
    public AssemblyEntity() : this(string.Empty, string.Empty, string.Empty, string.Empty) { }
}
