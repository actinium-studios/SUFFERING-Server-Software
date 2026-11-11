using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace SUFFERING_Server_Software;

public sealed class EditorSink(LineEditor editor, string outputTemplate) : ILogEventSink
{
    private readonly MessageTemplateTextFormatter _formatter = new(outputTemplate);

    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        _formatter.Format(logEvent, writer);
        editor.WriteLogLine(writer.ToString().TrimEnd('\r', '\n'));
    }
}
