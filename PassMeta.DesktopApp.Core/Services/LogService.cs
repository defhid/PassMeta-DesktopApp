namespace PassMeta.DesktopApp.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common.Interfaces.Services;
    using Common.Models.Entities;
    using Splat;
    using Utils;

    /// <inheritdoc />
    public class LogService : ILogService
    {
        private static readonly string Folder = AppConfig.LogFilesPath;

        private FileStream? _fileStream;
        private DateTime? _fileStreamDateOpened;

        /// <inheritdoc />
        public void Info(string text)
            => _Write(new Log { Section = "INFO", Text = text });

        /// <inheritdoc />
        public void Warning(string text)
            => _Write(new Log { Section = "WARNING", Text = text });
        
        /// <inheritdoc />
        public void Error(string text)
            => _Write(new Log { Section = "ERROR", Text = text });

        /// <inheritdoc />
        public void Error(Exception ex, string? text = null)
            => Error(text is null ? ex.ToString() : text + $" [{ex}]");

        /// <inheritdoc />
        public async Task<List<Log>> ReadLogsAsync(DateTime dateFrom, DateTime dateTo)
        {
            var dates = new List<DateTime> { dateFrom };
            var curr = dateFrom;
            
            while (curr < dateTo)
            {
                curr = curr.AddMonths(1);
                dates.Add(curr);
            }

            var logs = new List<Log>();

            foreach (var fileName in dates.Select(_GetFileNameFor))
            {
                try
                {
                    if (!File.Exists(fileName)) continue;
                    
                    var lines = await File.ReadAllLinesAsync(fileName);
                    foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
                    {
                        try
                        {
                            var i1 = line.IndexOf('|');
                            var i2 = line.IndexOf('|', i1);

                            logs.Add(new Log
                            {
                                Section = line[..i1],
                                CreatedOn = DateTime.Parse(line[(i1 + 1)..i2]),
                                Text = line[(i2 + 1)..]
                            });
                        }
                        catch
                        {
                            logs.Add(new Log
                            {
                                Section = "UNKNOWN",
                                CreatedOn = DateTime.MinValue,
                                Text = line
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Locator.Current.GetService<IDialogService>()!
                        .ShowError($"Can't read log file '{fileName}'", more: ex.ToString());
                }
            }

            return logs;
        }

        private void _Write(Log log)
        {
            log.Section ??= "UNKNOWN";
            log.Text ??= "?";
            log.CreatedOn ??= DateTime.Now;
            
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            _CheckFileStream();
            _fileStream!.Write(Encoding.UTF8.GetBytes(log.Section + '|' 
                                                      + log.CreatedOn.Value.ToString("yyyy-MM-dd hh:mm:ss") + '|' 
                                                      + log.Text.Replace("\n", " ") + '\n'));
            _fileStream.Flush();
        }
        
        private static string _GetFileNameFor(DateTime date) => Path.Combine(Folder, $"{date:yyyy-MM}.log");

        private void _CheckFileStream()
        {
            var today = DateTime.Today;
            
            if (_fileStreamDateOpened is null || _fileStreamDateOpened.Value.Month != today.Month)
            {
                _fileStream?.Close();
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
            _fileStream?.Close();
            _fileStream?.Dispose();
            _fileStream = null;
            GC.SuppressFinalize(this);
        }
    }
}