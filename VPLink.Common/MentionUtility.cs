using System.Globalization;
using System.Text;
using Cysharp.Text;
using Discord;
using VPLink.Common.Data;

namespace VPLink.Common;

public static class MentionUtility
{
    public static void ParseTag(IGuild guild,
        ReadOnlySpan<char> contents,
        ref PlainTextMessageBuilder builder,
        char whitespaceTrivia)
    {
        if (contents[..2].Equals("@&", StringComparison.Ordinal)) // role mention
        {
            ParseRoleMention(guild, contents, ref builder, whitespaceTrivia);
        }
        else if (contents[..2].Equals("t:", StringComparison.Ordinal)) // timestamp
        {
            ParseTimestamp(contents, ref builder, whitespaceTrivia);
        }
        else
            switch (contents[0])
            {
                // user mention
                case '@':
                    ParseUserMention(guild, contents, ref builder, whitespaceTrivia);
                    break;

                // channel mention
                case '#':
                    ParseChannelMention(guild, contents, ref builder, whitespaceTrivia);
                    break;

                default:
                    builder.AddWord($"<{contents.ToString()}>", whitespaceTrivia);
                    break;
            }
    }

    private static void ParseChannelMention(IGuild guild,
        ReadOnlySpan<char> contents,
        ref PlainTextMessageBuilder builder,
        char whitespaceTrivia)
    {
        ulong channelId = ulong.Parse(contents[1..]);
        ITextChannel? channel = guild.GetTextChannelAsync(channelId).GetAwaiter().GetResult();
        builder.AddWord(channel is null ? $"<{contents}>" : $"#{channel.Name}", whitespaceTrivia);
    }

    private static void ParseRoleMention(IGuild guild,
        ReadOnlySpan<char> contents,
        ref PlainTextMessageBuilder builder,
        char whitespaceTrivia)
    {
        ulong roleId = ulong.Parse(contents[2..]);
        IRole? role = guild.GetRole(roleId);
        builder.AddWord(role is null ? $"<{contents}>" : $"@{role.Name}", whitespaceTrivia);
    }

    private static void ParseUserMention(IGuild guild,
        ReadOnlySpan<char> contents,
        ref PlainTextMessageBuilder builder,
        char whitespaceTrivia)
    {
        ulong userId = ulong.Parse(contents[1..]);
        IGuildUser? user = guild.GetUserAsync(userId).GetAwaiter().GetResult();
        builder.AddWord(user is null ? $"<{contents}>" : $"@{user.Nickname ?? user.GlobalName ?? user.Username}",
            whitespaceTrivia);
    }

    private static void ParseTimestamp(ReadOnlySpan<char> contents,
        ref PlainTextMessageBuilder builder,
        char whitespaceTrivia)
    {
        using Utf8ValueStringBuilder buffer = ZString.CreateUtf8StringBuilder();
        var formatSpecifier = '\0';
        var isEscaped = false;
        var breakLoop = false;

        for (var index = 2; index < contents.Length; index++)
        {
            if (breakLoop)
            {
                break;
            }

            char current = contents[index];
            switch (current)
            {
                case '\\':
                    isEscaped = !isEscaped;
                    break;

                case ':' when !isEscaped && index + 1 < contents.Length:
                    formatSpecifier = contents[index + 1];
                    if (formatSpecifier == '>') formatSpecifier = '\0'; // ignore closing tag
                    breakLoop = true;
                    break;

                case '>' when !isEscaped:
                    break;

                case var _ when char.IsDigit(current):
                    buffer.Append(current);
                    break;

                default:
                    return;
            }
        }

        ReadOnlySpan<byte> bytes = buffer.AsSpan();
        int charCount = Encoding.UTF8.GetCharCount(bytes);
        Span<char> chars = stackalloc char[charCount];
        Encoding.UTF8.GetChars(bytes, chars);

        DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(chars, CultureInfo.InvariantCulture));
        builder.AddTimestamp(timestamp, (TimestampFormat)formatSpecifier, whitespaceTrivia);
    }
}
