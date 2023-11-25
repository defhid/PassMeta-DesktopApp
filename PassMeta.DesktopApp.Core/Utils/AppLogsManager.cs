using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging.Extra;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Core.Utils;

/// <inheritdoc />
public class AppLogsManager : ILogsManager
{
    private const char Separator = '|';
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private const int LogLifeTimeMonths = 2;

    private readonly string _logsDirectory;
    private readonly string _fileStreamPath;
    private readonly Queue<Log> _buffer = new(16);
    private StreamWriter? _fileWriter;

    /// <summary></summary>
    public AppLogsManager(string rootPath)
    {
        _logsDirectory = Path.Combine(rootPath, ".logs");
        _fileStreamPath = GetFilePathFor(DateTime.Today);
    }

    /// <inheritdoc />
    public void Write(Log log)
    {
        log.Section ??= Log.Sections.Unknown;
        log.Text ??= "?";
        log.CreatedOn ??= DateTime.Now;

        bool needHandle;
        lock (_buffer)
        {
            _buffer.Enqueue(log);
            needHandle = _buffer.Count == 1;
        }

        if (needHandle)
        {
            _ = HandleQueueAsync();
        }
    }

    /// <inheritdoc />
    public void Flush() => _fileWriter?.Flush();

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

        foreach (var filePath in dates.Select(GetFilePathFor))
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    continue;
                }

                using var reader = new StreamReader(filePath, Encoding.UTF8, false, new FileStreamOptions
                {
                    Share = FileShare.ReadWrite,
                    BufferSize = 8192,
                });

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var i1 = line.IndexOf(Separator);
                    var i2 = line.IndexOf(Separator, i1 + 1);

                    if (i1 > 0 && i2 > 0 && 
                        DateTime.TryParseExact(line[(i1 + 1)..i2], DateTimeFormat, 
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
            }
            catch (Exception ex)
            {
                EmitErrorOccured($"Can't read log file '{filePath}'", ex);
            }
        }

        return logs;
    }
        
    /// <inheritdoc />
    public Task CleanUpAsync()
    {
        try
        {
            var logFiles = Directory.EnumerateFiles(_logsDirectory).ToList();

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
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public event EventHandler<LoggerErrorEventArgs>? InternalErrorOccured;

    /// <summary></summary>
    ~AppLogsManager() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        _fileWriter?.Dispose();
        _fileWriter = null;
        GC.SuppressFinalize(this);
    }

    private async Task HandleQueueAsync()
    {
        while (true)
        {
            Log log;
            lock (_buffer)
            {
                log = _buffer.Peek();
            }

            try
            {
                if (_fileWriter is null)
                {
                    Directory.CreateDirectory(_logsDirectory);
                    _fileWriter ??= new StreamWriter(_fileStreamPath, Encoding.UTF8, new FileStreamOptions
                    {
                        Mode = FileMode.Append,
                        Access = FileAccess.Write,
                        Share = FileShare.ReadWrite,
                        BufferSize = 8192,
                    });
                }

                await _fileWriter.WriteAsync(
                    log.Section + Separator +
                    log.CreatedOn!
                        .Value
                        .ToString(DateTimeFormat, CultureInfo.InvariantCulture) + Separator +
                    log.Text!
                        .Replace("\n", " ")
                        .Replace("\r", " ")
                        .Replace(Separator.ToString(), " ") +
                    Environment.NewLine);

                // ReSharper disable once InconsistentlySynchronizedField
                if (_buffer.Count == 1)
                {
                    await _fileWriter.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                EmitErrorOccured($"Can't log to '{_fileStreamPath}' a message: '{log.Text}'", ex);
            }

            lock (_buffer)
            {
                _buffer.Dequeue();

                if (_buffer.Count == 0)
                {
                    break;
                }
            }
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

    private string GetFilePathFor(DateTime date) => Path.Combine(_logsDirectory, $"{date:yyyy-MM}.log");
}