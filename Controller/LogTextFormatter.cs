using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Controller;

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
