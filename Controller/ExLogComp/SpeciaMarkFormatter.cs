namespace Controller.ExLogComp;

/// <summary>
/// dong hui
/// 2026-09-27 12:39
/// 初版
/// 扩展日志滤除器
/// </summary>
public class SpeciaMarkFormatter(ITextFormatter TextFormatter,
                                 string RemoveStr,
                                 StringComparison _tringComparison = StringComparison.Ordinal) : ITextFormatter
{
    private readonly ITextFormatter _ITextFormatter = TextFormatter;

    private readonly string _RemoveStr = RemoveStr;

    private readonly StringComparison _StringComparison = _tringComparison;

    public void Format(LogEvent logEvent, TextWriter output)
    {
        StringWriter NewStr = new();

        _ITextFormatter.Format(logEvent, NewStr);

        output.Write(NewStr.ToString().Replace(_RemoveStr, string.Empty, _StringComparison));
    }
}
