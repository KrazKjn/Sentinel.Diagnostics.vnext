using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using System;
using System.IO;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal
{
    internal sealed class FileSentinelLogger : SentinelLoggerBase
    {
        private readonly string _path;
        private readonly object _lock = new();

        public FileSentinelLogger(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));

            // Ensure directory exists
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
        }

        protected override void Write(AutoLoggerLevel autoLoggerLevel, string message)
        {
            lock (_lock)
            {
                File.AppendAllText(_path, message + Environment.NewLine);
            }
        }
    }
}
