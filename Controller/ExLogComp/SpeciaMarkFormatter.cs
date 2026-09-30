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

    private readonly ObjectPool<StringWriter> NewStrWriter = new DefaultObjectPoolProvider().Create<StringWriter>();

    public void Format(LogEvent logEvent, TextWriter output)
    {
        try
        {
            StringWriter Newstr = NewStrWriter.Get();

            Newstr.GetStringBuilder().Clear();

            _ITextFormatter.Format(logEvent, Newstr);

            output.Write(Newstr.ToString().Replace(_RemoveStr, string.Empty, _StringComparison));

            Newstr.GetStringBuilder().Clear();

            NewStrWriter.Return(Newstr);

        }
        catch (Exception ex)
        {
            ex.Message.ToString();
        }
    }
}
