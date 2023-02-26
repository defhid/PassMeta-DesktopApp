using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging.Extra;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class LogsManager : ILogsManager
{
    private static readonly string LogsDirectory = Path.Combine(AppInfo.RootPath, ".logs");
    private readonly object _lockObject = new();

    private const char Separator = '|';
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private const int LogLifeTimeMonths = 2;

    private FileStream? _fileStream;
    private DateTime? _fileStreamDateOpened;

    /// <inheritdoc />
    public void Write(Log log)
    {
        log.Section ??= Log.Sections.Unknown;
        log.Text ??= "?";
        log.CreatedOn ??= DateTime.Now;
        
#if DEBUG
        Console.WriteLine($@"{log.CreatedOn:hh:mm:ss} {log.Section} {log.Text}");
#endif

        try
        {
            lock (_lockObject)
            {
                EnsureFileStreamActual();

                _fileStream!.Write(Encoding.UTF8.GetBytes(
                    log.Section + Separator +
                    log.CreatedOn
                        .Value
                        .ToString(DateTimeFormat, CultureInfo.InvariantCulture) + Separator +
                    log.Text
                        .Replace("\n", " ")
                        .Replace("\r", " ")
                        .Replace(Separator.ToString(), " ") +
                    Environment.NewLine));

                _fileStream.Flush();
            }
        }
        catch (Exception ex)
        {
            EmitErrorOccured($"Can't log to file '{GetFileNameFor(log.CreatedOn.Value)}'", ex);
        }
    }

    /// <inheritdoc />
    public IAppConfigProvider? AppConfigProvider { get; set; }

    /// <inheritdoc />
    public List<Log> Read(DateTime dateFrom, DateTime dateTo)
    {
        var dates = new List<DateTime> { dateFrom };
        var curr = dateFrom;
            
        while (curr < dateTo)
        {
            curr = curr.AddMonths(1);
            dates.Add(curr);
        }

        var logs = new List<Log>();

        foreach (var fileName in dates.Select(GetFileNameFor))
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    continue;
                }
                    
                lock (_lockObject)
                {
                    _fileStream?.Dispose();
                    _fileStreamDateOpened = null;
                        
                    using var stream = new StreamReader(fileName, Encoding.UTF8);
                    while (!stream.EndOfStream)
                    {
                        var line = stream.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        var i1 = line.IndexOf(Separator);
                        var i2 = line.IndexOf(Separator, i1 + 1);

                        if (i1 > 0 && i2 > 0 && DateTime.TryParseExact(line[(i1 + 1)..i2], DateTimeFormat, 
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out var createdOn))
                        {
                            if (createdOn < dateFrom) continue;
                            if (createdOn.Date > dateTo) break;

                            logs.Add(new Log
                            {
                                Section = line[..i1],
                                CreatedOn = createdOn,
                                Text = line[(i2 + 1)..]
                            });
                        }
                    }
                    
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                EmitErrorOccured($"Can't read log from file '{fileName}'", ex);
            }
        }

        return logs;
    }
        
    /// <inheritdoc />
    public void CleanUp()
    {
        try
        {
            var logFiles = Directory.EnumerateFiles(LogsDirectory).ToList();

            for (var i = logFiles.Count - 1; i >= 0; --i)
            {
                if (!logFiles[i].EndsWith(".log"))
                {
                    File.Delete(logFiles[i]);
                    logFiles.RemoveAt(i);
                }
            }

            logFiles.Sort(StringComparer.Ordinal);

            for (var i = logFiles.Count - 1 - LogLifeTimeMonths; i >= 0; --i)
            {
                File.Delete(logFiles[i]);
            }
        }
        catch (Exception ex)
        {
            Write(new Log
            {
                Section = Log.Sections.Error, 
                Text = $"Logs optimizing failed [{ex}]"
            });
        }
    }

    /// <inheritdoc />
    public event EventHandler<LoggerErrorEventArgs>? InternalErrorOccured;

    /// <summary></summary>
    ~LogsManager() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        _fileStream?.Dispose();
        _fileStream = null;
        GC.SuppressFinalize(this);
    }

    private void EnsureFileStreamActual()
    {
        var today = DateTime.Today;
            
        if (_fileStreamDateOpened is null || 
            _fileStreamDateOpened.Value.Month != today.Month)
        {
            _fileStream?.Dispose();
            _fileStream = new FileStream(GetFileNameFor(today), FileMode.Append);
            _fileStreamDateOpened = today;
        }
    }

    private void EmitErrorOccured(string message, Exception ex)
    {
        try
        {
            InternalErrorOccured?.Invoke(this, new LoggerErrorEventArgs(message, ex));
        }
        catch
        {
            // ignored
        }
    }

    private static string GetFileNameFor(DateTime date) => $"{date:yyyy-MM}.log";
}