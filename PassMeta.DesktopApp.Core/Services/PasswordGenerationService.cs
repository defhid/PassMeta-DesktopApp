using System;
using System.Collections.Generic;
using System.Linq;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class PasswordGenerationService : IPasswordGenerationService
{
    private const string FailureGenerationResult = ":(";
    private static readonly Random Random = new();
    private readonly ILogService _logger;

    /// <summary></summary>
    public PasswordGenerationService(ILogService logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string GeneratePassword(int length, bool digits, bool lowercase, bool uppercase, bool special)
    {
        const string userFriendlyLowercaseSet = "abcdefghijkmnopqrstuvwxyz";
        const string userFriendlyUppercaseSet = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string userFriendlySet = userFriendlyLowercaseSet + userFriendlyUppercaseSet;
        const string digitSet = "0123456789";
        const string specialSet = "*-_!@";

        var builder = Enumerable.Repeat(string.Empty, 0);

        var letters = lowercase || uppercase;
        var k = (float)Math.Max((lowercase ? 1 : 0) + (uppercase ? 1 : 0), 1) / 2;

        if (!digits && !letters && !special)
        {
            return FailureGenerationResult;
        }

        try
        {
            if (digits)
                builder = builder.Concat(Enumerable.Repeat(digitSet, (int)(5 * k)));

            if (lowercase)
                builder = builder.Concat(Enumerable.Repeat(userFriendlyLowercaseSet, 2));

            if (uppercase)
                builder = builder.Concat(Enumerable.Repeat(userFriendlyUppercaseSet, 2));

            if (special)
                builder = builder.Concat(Enumerable.Repeat(specialSet, (int)(5 * k)));

            var chars = string.Concat(builder.OrderBy(_ => Random.Next()));

            var stack = new Stack<char>(length);

            var result = new string(Enumerable.Repeat(chars, length * 20)
                .Select(s => s[Random.Next(s.Length)])
                .SkipWhile(letters
                    ? s => !userFriendlySet.Contains(s)
                    : _ => false)
                .Where(letters || digits
                    ? s =>
                    {
                        if (stack.TryPeek(out var prev) && specialSet.Contains(prev) && specialSet.Contains(s))
                        {
                            return false;
                        }

                        stack.Push(s);
                        return true;
                    }
                    : _ => true)
                .Take(length)
                .ToArray());

            return result.Length < length
                ? GeneratePassword(length, digits, lowercase, uppercase, special)
                : result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Password generation failed");
            return FailureGenerationResult;
        }
    }
}