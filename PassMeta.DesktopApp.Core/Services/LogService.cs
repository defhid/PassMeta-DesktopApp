namespace PassMeta.DesktopApp.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Common.Abstractions.Services;
    using Common.Models.Entities;

    /// <inheritdoc />
    public class LogService : ILogService
    {
        private static readonly string Folder = AppConfig.LogFilesDirectory;

        private FileStream? _fileStream;
        private DateTime? _fileStreamDateOpened;
        
        private readonly object _lockObject = new();

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const int LogLifeTimeMonths = 2;

        /// <inheritdoc />
        public void Info(string text)
            => _Write(new Log { Section = Log.Sections.Info, Text = text });

        /// <inheritdoc />
        public void Warning(string text)
            => _Write(new Log { Section = Log.Sections.Warning, Text = text });
        
        /// <inheritdoc />
        public void Error(string text)
            => _Write(new Log { Section = Log.Sections.Error, Text = text });

        /// <inheritdoc />
        public void Error(Exception ex, string? text = null)
            => Error(text is null ? ex.ToString() : text + $" [{ex}]");

        /// <inheritdoc />
        public List<Log> ReadLogs(DateTime dateFrom, DateTime dateTo)
        {
            var dates = new List<DateTime> { dateFrom };
            var curr = dateFrom;
            
            while (curr < dateTo)
            {
                curr = curr.AddMonths(1);
                dates.Add(curr);
            }

            var logs = new List<Log>();
            var errors = new List<(string, Exception)>();

            foreach (var fileName in dates.Select(_GetFileNameFor))
            {
                try
                {
                    if (!File.Exists(fileName)) continue;
                    
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

                            var i1 = line.IndexOf('|');
                            var i2 = line.IndexOf('|', i1 + 1);

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
                    errors.Add(($"Can't read log file '{fileName}'", ex));
                }
            }

            foreach (var (text, exception) in errors)
            {
                EnvironmentContainer.Resolve<IDialogService>()
                    .ShowError(text, more: exception.ToString());
            }

            return logs;
        }
        
        /// <inheritdoc />
        public void OptimizeLogs()
        {
            try
            {
                var logFiles = Directory.EnumerateFiles(Folder).ToList();

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
                Error(ex, "Logs optimizing failed");
            }
        }

        private void _Write(Log log)
        {
            log.Section ??= Log.Sections.Unknown;
            log.Text ??= "?";
            log.CreatedOn ??= DateTime.Now;
            
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            lock (_lockObject)
            {
                _CheckFileStream();
                _fileStream!.Write(Encoding.UTF8.GetBytes(log.Section + '|' 
                                                                      + log.CreatedOn.Value.ToString(DateTimeFormat, CultureInfo.InvariantCulture) + '|' 
                                                                      + log.Text.Replace(Environment.NewLine, " ") + Environment.NewLine));
                _fileStream.Flush();
            }
        }
        
        private static string _GetFileNameFor(DateTime date) => Path.Combine(Folder, $"{date:yyyy-MM}.log");

        private void _CheckFileStream()
        {
            var today = DateTime.Today;
            
            if (_fileStreamDateOpened is null || _fileStreamDateOpened.Value.Month != today.Month)
            {
                _fileStream?.Dispose();
                _fileStream = new FileStream(_GetFileNameFor(today), FileMode.Append);
                _fileStreamDateOpened = today;
            }
        }

        /// <summary></summary>
        ~LogService() => Dispose();

        /// <inheritdoc />
        public void Dispose()
        {
            _fileStream?.Dispose();
            _fileStream = null;
            GC.SuppressFinalize(this);
        }
    }
}