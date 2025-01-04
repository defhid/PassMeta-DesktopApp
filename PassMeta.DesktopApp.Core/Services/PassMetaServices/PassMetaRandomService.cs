using System;
using System.Linq;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;

namespace PassMeta.DesktopApp.Core.Services.PassMetaServices;

/// <inheritdoc />
public class PassMetaRandomService : IPassMetaRandomService
{
    private const string FailureGenerationResult = ":(";
    private static readonly Random Random = new();
    private readonly ILogsWriter _logger;

    /// <summary></summary>
    public PassMetaRandomService(ILogsWriter logger)
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

        if (!digits && !letters && !special)
        {
            return FailureGenerationResult;
        }

        try
        {
            if (lowercase)
                builder = builder.Concat(Enumerable.Repeat(userFriendlyLowercaseSet, 2));

            if (uppercase)
                builder = builder.Concat(Enumerable.Repeat(userFriendlyUppercaseSet, 2));

            if (digits)
            {
                var k = Math.Max((lowercase ? 1 : 0) + (uppercase ? 1 : 0), 1);

                builder = builder.Concat(Enumerable.Repeat(digitSet, k));
            }

            if (special)
            {
                var k = Math.Max((lowercase ? 1 : 0) + (uppercase ? 1 : 0) + (digits ? 1 : 0), 1);

                builder = builder.Concat(Enumerable.Repeat(specialSet, k));
            }

            var chars = string.Concat(builder.OrderBy(_ => Random.Next()));
            char? prev = null;

            var result = new string(Enumerable.Repeat(chars, length * 20)
                .Select(s => s[Random.Next(s.Length)])
                .SkipWhile(letters
                    ? s => !userFriendlySet.Contains(s)
                    : _ => false)
                .Where(letters || digits
                    ? s =>
                    {
                        if (prev is not null)
                        {
                            if (specialSet.Contains(prev.Value) && specialSet.Contains(s))
                            {
                                return false;
                            }
                        }

                        prev = s;
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