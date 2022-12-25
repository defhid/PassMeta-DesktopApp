using System;
using System.Reflection;
using PassMeta.DesktopApp.Common.Constants;

namespace PassMeta.DesktopApp.Core;

/// <summary>
/// Information about this application.
/// </summary>
public static class AppInfo
{
    private const int CopyrightFirstYear = 2022;

    /// <summary>
    /// Application author.
    /// </summary>
    public const string Author = "Vladislav Mironov";

    /// <summary>
    /// Copyright text + years.
    /// </summary>
    public static readonly string Copyright = $"Copyright © {CopyrightFirstYear}" + 
                                              (DateTime.Now.Year > CopyrightFirstYear ? "-" + DateTime.Now.Year : string.Empty);

    /// <summary>
    /// Application version.
    /// </summary>
    public static readonly string Version = Assembly.GetAssembly(typeof(AppCulture))!.GetName().Version?.ToString()[..^2] ?? "?";

    /// <summary>
    /// x64/x86.
    /// </summary>
    public static readonly string Bit = Environment.Is64BitProcess ? "64-bit" : "32-bit";
}