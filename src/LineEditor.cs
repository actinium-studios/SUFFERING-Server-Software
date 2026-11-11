using System.Text;

namespace SUFFERING_Server_Software;

public sealed class LineEditor(Func<IEnumerable<string>>? completions = null)
{
    private const string Prompt = "> ";
    private const int MaxHistory = 200;
 
    private readonly List<string> _history = [];
 
    public string? ReadLine(CancellationToken ct)
    {
        if (Console.IsInputRedirected)
            return Console.ReadLine();
 
        var buf = new StringBuilder();
        var cursor = 0;
        var drawnLength = 0;
        var historyIndex = _history.Count;
        var draft = "";
 
        void Redraw()
        {
            var text = buf.ToString();
            var pad = Math.Max(0, drawnLength - text.Length);
            Console.Write("\r" + Prompt + text + new string(' ', pad));
            Console.CursorLeft = Math.Min(Prompt.Length + cursor, Math.Max(0, Console.BufferWidth - 1));
            drawnLength = text.Length;
        }
 
        void SetBuffer(string s)
        {
            buf.Clear().Append(s);
            cursor = s.Length;
            Redraw();
        }
 
        Redraw();
 
        while (!ct.IsCancellationRequested)
        {
            if (!Console.KeyAvailable)
            {
                Thread.Sleep(10);
                continue;
            }
 
            var key = Console.ReadKey(intercept: true);
            var ctrl = (key.Modifiers & ConsoleModifiers.Control) != 0;
 
            if (ctrl && key.Key == ConsoleKey.U) { SetBuffer(""); continue; }
            if (ctrl && key.Key == ConsoleKey.A) { cursor = 0; Redraw(); continue; }
            if (ctrl && key.Key == ConsoleKey.E) { cursor = buf.Length; Redraw(); continue; }
 
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                {
                    Console.WriteLine();
                    var line = buf.ToString();
                    if (line.Trim().Length > 0 && (_history.Count == 0 || _history[^1] != line))
                    {
                        _history.Add(line);
                        if (_history.Count > MaxHistory) _history.RemoveAt(0);
                    }
                    return line;
                }
 
                case ConsoleKey.UpArrow:
                    if (historyIndex > 0)
                    {
                        if (historyIndex == _history.Count) draft = buf.ToString();
                        historyIndex--;
                        SetBuffer(_history[historyIndex]);
                    }
                    break;
 
                case ConsoleKey.DownArrow:
                    if (historyIndex < _history.Count)
                    {
                        historyIndex++;
                        SetBuffer(historyIndex == _history.Count ? draft : _history[historyIndex]);
                    }
                    break;
 
                case ConsoleKey.LeftArrow:
                    if (cursor > 0) { cursor--; Redraw(); }
                    break;
 
                case ConsoleKey.RightArrow:
                    if (cursor < buf.Length) { cursor++; Redraw(); }
                    break;
 
                case ConsoleKey.Home:
                    cursor = 0;
                    Redraw();
                    break;
 
                case ConsoleKey.End:
                    cursor = buf.Length;
                    Redraw();
                    break;
 
                case ConsoleKey.Backspace:
                    if (cursor > 0)
                    {
                        buf.Remove(cursor - 1, 1);
                        cursor--;
                        Redraw();
                    }
                    break;
 
                case ConsoleKey.Delete:
                    if (cursor < buf.Length)
                    {
                        buf.Remove(cursor, 1);
                        Redraw();
                    }
                    break;
 
                case ConsoleKey.Escape:
                    SetBuffer("");
                    break;
 
                case ConsoleKey.Tab:
                {
                    var text = buf.ToString();
                    if (completions is null || text.Contains(' ')) break;
 
                    var matches = completions()
                        .Where(n => n.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(n => n)
                        .ToList();
 
                    if (matches.Count == 1)
                    {
                        SetBuffer(matches[0] + " ");
                    }
                    else if (matches.Count > 1)
                    {
                        var prefix = matches[0];
                        foreach (var m in matches)
                            while (!m.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                prefix = prefix[..^1];
                        SetBuffer(prefix);
                    }
                    break;
                }
 
                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        buf.Insert(cursor, key.KeyChar);
                        cursor++;
                        Redraw();
                    }
                    break;
            }
        }
 
        return null;
    }
}
