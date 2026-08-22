using log4net;
using log4net.Config;
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;
using System.IO;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal
{
    internal sealed class Log4NetSentinelLogger : SentinelLoggerBase
    {
        private readonly ILog _log;

        public static readonly Log4NetSentinelLogger Instance = new();

        public Log4NetSentinelLogger(string? configPath = null)
        {
            if (!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath))
                XmlConfigurator.Configure(new FileInfo(configPath));
            else
                XmlConfigurator.Configure();

            _log = LogManager.GetLogger(typeof(Log4NetSentinelLogger));
        }

        protected override void Write(AutoLoggerLevel autoLoggerLevel, string message, SentinelLogEvent evt)
        {
            switch (autoLoggerLevel.Level)
            {
                case SentinelLogLevel.Trace:
                    _log.Debug(message); // Log4Net has no Trace
                    break;

                case SentinelLogLevel.Debug:
                    _log.Debug(message);
                    break;

                case SentinelLogLevel.Information:
                    _log.Info(message);
                    break;

                case SentinelLogLevel.Warning:
                    _log.Warn(message);
                    break;

                case SentinelLogLevel.Error:
                    _log.Error(message);
                    break;

                case SentinelLogLevel.Critical:
                    _log.Fatal(message);
                    break;

                default:
                    _log.Info(message);
                    break;
            }
        }
    }
}