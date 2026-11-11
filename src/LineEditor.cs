using System.Text;

namespace SUFFERING_Server_Software;

public sealed class LineEditor
{
    private const string Prompt = "> ";
    private const int MaxHistory = 200;

    private readonly object _lock = new();
    private readonly List<string> _history = [];
    private readonly StringBuilder _buf = new();

    private int _cursor;
    private int _drawnLength;
    private int _historyIndex;
    private string _draft = "";
    private bool _active;

    public Func<string, IEnumerable<string>>? Completions { get; set; }

    public void WriteLogLine(string line)
    {
        lock (_lock)
        {
            if (_active) ClearLine();
            Console.WriteLine(line);
            if (_active) Redraw();
        }
    }

    public string? ReadLine(CancellationToken ct)
    {
        if (Console.IsInputRedirected)
            return Console.ReadLine();

        lock (_lock)
        {
            _buf.Clear();
            _cursor = 0;
            _drawnLength = 0;
            _historyIndex = _history.Count;
            _draft = "";
            _active = true;
            Redraw();
        }

        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(10);
                    continue;
                }

                var key = Console.ReadKey(intercept: true);
                lock (_lock)
                {
                    var line = HandleKey(key);
                    if (line is not null) return line;
                }
            }

            return null;
        }
        finally
        {
            lock (_lock)
            {
                if (_active && ct.IsCancellationRequested) ClearLine();
                _active = false;
            }
        }
    }

    private string? HandleKey(ConsoleKeyInfo key)
    {
        var ctrl = (key.Modifiers & ConsoleModifiers.Control) != 0;

        if (ctrl && key.Key == ConsoleKey.U) { SetBuffer(""); return null; }
        if (ctrl && key.Key == ConsoleKey.A) { _cursor = 0; Redraw(); return null; }
        if (ctrl && key.Key == ConsoleKey.E) { _cursor = _buf.Length; Redraw(); return null; }

        switch (key.Key)
        {
            case ConsoleKey.Enter:
            {
                var line = _buf.ToString();
                Console.WriteLine();
                _drawnLength = 0;

                if (line.Trim().Length > 0 && (_history.Count == 0 || _history[^1] != line))
                {
                    _history.Add(line);
                    if (_history.Count > MaxHistory) _history.RemoveAt(0);
                }
                return line;
            }

            case ConsoleKey.UpArrow:
                if (_historyIndex > 0)
                {
                    if (_historyIndex == _history.Count) _draft = _buf.ToString();
                    _historyIndex--;
                    SetBuffer(_history[_historyIndex]);
                }
                break;

            case ConsoleKey.DownArrow:
                if (_historyIndex < _history.Count)
                {
                    _historyIndex++;
                    SetBuffer(_historyIndex == _history.Count ? _draft : _history[_historyIndex]);
                }
                break;

            case ConsoleKey.LeftArrow:
                if (_cursor > 0) { _cursor--; Redraw(); }
                break;

            case ConsoleKey.RightArrow:
                if (_cursor < _buf.Length) { _cursor++; Redraw(); }
                break;

            case ConsoleKey.Home:
                _cursor = 0;
                Redraw();
                break;

            case ConsoleKey.End:
                _cursor = _buf.Length;
                Redraw();
                break;

            case ConsoleKey.Backspace:
                if (_cursor > 0)
                {
                    _buf.Remove(_cursor - 1, 1);
                    _cursor--;
                    Redraw();
                }
                break;

            case ConsoleKey.Delete:
                if (_cursor < _buf.Length)
                {
                    _buf.Remove(_cursor, 1);
                    Redraw();
                }
                break;

            case ConsoleKey.Escape:
                SetBuffer("");
                break;

            case ConsoleKey.Tab:
                Complete();
                break;

            default:
                if (!char.IsControl(key.KeyChar))
                {
                    _buf.Insert(_cursor, key.KeyChar);
                    _cursor++;
                    Redraw();
                }
                break;
        }

        return null;
    }

    private void Complete()
    {
        if (Completions is null || _cursor != _buf.Length) return;

        var text = _buf.ToString();
        List<string> matches;
        try
        {
            matches = Completions(text)
                .Where(c => c.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception)
        {
            return;
        }

        if (matches.Count == 0) return;

        if (matches.Count == 1)
        {
            SetBuffer(matches[0].EndsWith(' ') ? matches[0] : matches[0] + " ");
            return;
        }

        var prefix = matches[0];
        foreach (var m in matches)
            while (!m.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                prefix = prefix[..^1];

        if (prefix.Length > text.Length)
        {
            SetBuffer(prefix);
            return;
        }

        ClearLine();
        Console.WriteLine(string.Join("  ", matches.Select(m => m[(m.LastIndexOf(' ') + 1)..])));
        Redraw();
    }

    private void SetBuffer(string s)
    {
        _buf.Clear().Append(s);
        _cursor = s.Length;
        Redraw();
    }

    private void ClearLine()
    {
        Console.Write("\r" + new string(' ', Prompt.Length + _drawnLength) + "\r");
        _drawnLength = 0;
    }

    private void Redraw()
    {
        var text = _buf.ToString();
        var pad = Math.Max(0, _drawnLength - text.Length);
        Console.Write("\r" + Prompt + text + new string(' ', pad));
        Console.CursorLeft = Math.Min(Prompt.Length + _cursor, Math.Max(0, Console.BufferWidth - 1));
        _drawnLength = text.Length;
    }
}
