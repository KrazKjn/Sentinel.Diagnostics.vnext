using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;
using System;
using System.IO;
using System.Text.Json;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal
{
    internal sealed class StructuredJsonSentinelLogger : SentinelLoggerBase
    {
        private readonly string _path;
        private readonly object _lock = new();

        public StructuredJsonSentinelLogger(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
        }

        protected override void Write(AutoLoggerLevel autoLoggerLevel, string message, SentinelLogEvent evt)
        {
            // message is already formatted text — but JSON logger should
            // emit structured objects instead of raw strings.
            // So we wrap the message in a JSON envelope.
            DateTime dateTime;
            Guid? guid;
            int? indented;
            if (LogLineParser.TryParse(message, out var ts, out var id, out var indents, out var msg))
            {
                dateTime = ts;   // 2026-08-19T16:59:35.1334656Z
                guid = id;       // 85e34a78-31ab-4d7f-a865-eb14202c8db8
                indented = indents;
                message = msg;   // [Func......
            }
            else
            {
                dateTime = DateTime.UtcNow;
                guid = null;
                indented = null;
            }

            var json = JsonSerializer.Serialize(new
            {
                Timestamp = dateTime,
                Guid = guid,
                Indented = indented,
                Message = message
            });

            lock (_lock)
            {
                File.AppendAllText(_path, json + Environment.NewLine);
            }
        }
    }
}
