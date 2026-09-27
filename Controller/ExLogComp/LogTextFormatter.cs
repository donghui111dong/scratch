namespace Controller.ExLogComp;

/// <summary>
/// dong hui
/// 2026-09-27 11:24
/// 初版
/// 扩展日志记录器
/// </summary>
public class LogTextFormatter(string DefaultTemp,
                              string SpecialTemp,
                              Func<LogEvent, bool> MatchCon) : ITextFormatter
{
    private readonly MessageTemplateTextFormatter _DefaultTemp = new(DefaultTemp);

    private readonly MessageTemplateTextFormatter _SpecialTemp = new(SpecialTemp);

    private readonly Func<LogEvent, bool> _MatchCon = MatchCon;

    public void Format(LogEvent LogEbj, TextWriter OutMsg)
    {
        if (_MatchCon(LogEbj))
        {
            _SpecialTemp.Format(LogEbj, OutMsg);
        }
        else
        {
            _DefaultTemp.Format(LogEbj, OutMsg);
        }
    }
}
