using System;
using System.IO;
using System.Reflection;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Models.Internal;

namespace PassMeta.DesktopApp.Core;

/// <summary>
/// Factory for <see cref="AppInfo"/>.
/// </summary>
public static class AppInfoSource
{
    private const int CopyrightFirstYear = 2022;

    /// <summary>
    /// Current <see cref="AppInfo"/>.
    /// </summary>
    public static AppInfo Get() => new()
    {
        Author = "Vladislav Mironov",
        Copyright = $"Copyright © {CopyrightFirstYear}" +
                    (DateTime.Now.Year > CopyrightFirstYear ? "-" + DateTime.Now.Year : string.Empty),
        Version = Assembly.GetAssembly(typeof(AppCulture))!.GetName().Version?.ToString()[..^2] ?? "?",
        Bit = Environment.Is64BitProcess ? "64-bit" : "32-bit",
        RootPath = AppDomain.CurrentDomain.BaseDirectory
    };
}