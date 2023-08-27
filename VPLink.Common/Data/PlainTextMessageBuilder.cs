using Cysharp.Text;
using Humanizer;

namespace VPLink.Common.Data;

/// <summary>
///     Represents a plain text message builder.
/// </summary>
public struct PlainTextMessageBuilder : IDisposable
{
    private Utf8ValueStringBuilder _builder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainTextMessageBuilder" /> struct.
    /// </summary>
    public PlainTextMessageBuilder()
    {
        _builder = ZString.CreateUtf8StringBuilder();
    }

    /// <summary>
    ///     Appends the specified word.
    /// </summary>
    /// <param name="word">The word.</param>
    /// <param name="whitespace">The trailing whitespace trivia.</param>
    public void AddWord(ReadOnlySpan<char> word, char whitespace = ' ')
    {
        _builder.Append(word);
        if (whitespace != '\0') _builder.Append(whitespace);
    }

    /// <summary>
    ///     Appends the specified word.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <param name="format">The format.</param>
    /// <param name="whitespace">The trailing whitespace trivia.</param>
    public void AddTimestamp(DateTimeOffset timestamp, TimestampFormat format = TimestampFormat.None,
        char whitespace = ' ')
    {
        switch (format)
        {
            case TimestampFormat.Relative:
                AddWord(timestamp.Humanize(), whitespace);
                break;

            case TimestampFormat.None:
                AddWord(timestamp.ToString("d MMM yyyy HH:mm"));
                AddWord("UTC", whitespace);
                break;

            case TimestampFormat.LongDate:
                AddWord(timestamp.ToString("dd MMMM yyyy"));
                AddWord("UTC", whitespace);
                break;

            case TimestampFormat.ShortDate:
                AddWord(timestamp.ToString("dd/MM/yyyy"));
                AddWord("UTC", whitespace);
                break;

            case TimestampFormat.ShortTime:
                AddWord(timestamp.ToString("HH:mm"));
                AddWord("UTC", whitespace);
                break;

            case TimestampFormat.LongTime:
                AddWord(timestamp.ToString("HH:mm:ss"));
                AddWord("UTC", whitespace);
                break;

            case TimestampFormat.ShortDateTime:
                AddWord(timestamp.ToString("dd MMMM yyyy HH:mm"));
                AddWord("UTC", whitespace);
                break;

            case TimestampFormat.LongDateTime:
                AddWord(timestamp.ToString("dddd, dd MMMM yyyy HH:mm"));
                AddWord("UTC", whitespace);
                break;

            default:
                AddWord($"<t:{timestamp.ToUnixTimeSeconds():D}:{format}>", whitespace);
                break;
        }
    }

    /// <summary>
    ///     Clears the builder.
    /// </summary>
    public void Clear()
    {
        _builder.Clear();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _builder.Dispose();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _builder.ToString().Trim();
    }
}
