using System;
using System.Collections.Generic;
using System.Globalization;

namespace PassMeta.DesktopApp.Common.Constants;

/// <summary>
/// Application culture info.
/// </summary>
public readonly struct AppCulture
{
    private readonly string _code;
    private readonly Func<string> _nameGetter;
        
    /// <summary>
    /// Culture code.
    /// </summary>
    public string Code => _code;
        
    /// <summary>
    /// Culture-dependent culture name.
    /// </summary>
    public string Name => _nameGetter();

    private AppCulture(string code, Func<string> getName)
    {
        _code = code;
        _nameGetter = getName;
    }

    /// <summary>
    /// Get culture name.
    /// </summary>
    public override string ToString() => _nameGetter();
        
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AppCulture other && _code == other._code;

    /// <inheritdoc />
    public override int GetHashCode() => _code.GetHashCode();

    /// <summary>
    /// Try to get culture from <see cref="All"/> by <paramref name="code"/>.
    /// If failed, <paramref name="culture"/> will be <see cref="Default"/>.
    /// </summary>
    public static bool TryParse(string code, out AppCulture culture)
    {
        code = code.Trim().ToLower();
            
        foreach (var cult in All)
        {
            if (cult._code != code) continue;
            culture = cult;
            return true;
        }

        culture = Default;
        return false;
    }

    /// <summary>
    /// Cast to <see cref="CultureInfo"/> by <see cref="Code"/>.
    /// </summary>
    public static implicit operator CultureInfo(AppCulture culture) => new(culture._code);

    /// <summary></summary>
    public static bool operator ==(AppCulture first, AppCulture second) => first.Equals(second);

    /// <summary></summary>
    public static bool operator !=(AppCulture first, AppCulture second) => !first.Equals(second);

    #region Variants

    /// Russian.
    public static readonly AppCulture Ru = new("ru", () => Resources.LANG__RU);
        
    /// English.
    public static readonly AppCulture En = new("en", () => Resources.LANG__EN);

    /// <summary>
    /// Default application culture.
    /// </summary>
    public static readonly AppCulture Default = En;
        
    /// <summary>
    /// Application available culture list.
    /// </summary>
    public static readonly IReadOnlyList<AppCulture> All = new[] { Ru, En, };

    #endregion
}