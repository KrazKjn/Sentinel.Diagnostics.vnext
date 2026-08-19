using Microsoft.Extensions.Configuration;
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Serilog;
using Serilog.Events;
using System.IO;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal
{
    internal sealed class SerilogSentinelLogger : SentinelLoggerBase
    {
        private readonly Serilog.Core.Logger _logger;

        public SerilogSentinelLogger(string? configPath = null)
        {
            if (!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath))
            {
                // Load from JSON or XML config file
                _logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(
                        new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                            .AddJsonFile(configPath)
                            .Build())
                    .CreateLogger();
            }
            else
            {
                // Fallback: simple console logger
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger();
            }
        }

        protected override void Write(AutoLoggerLevel autoLoggerLevel, string message)
        {
            switch (autoLoggerLevel.Level)
            {
                case SentinelLogLevel.Trace:
                    _logger.Write(LogEventLevel.Verbose, message);
                    break;

                case SentinelLogLevel.Debug:
                    _logger.Write(LogEventLevel.Debug, message);
                    break;

                case SentinelLogLevel.Information:
                    _logger.Write(LogEventLevel.Information, message);
                    break;

                case SentinelLogLevel.Warning:
                    _logger.Write(LogEventLevel.Warning, message);
                    break;

                case SentinelLogLevel.Error:
                    _logger.Write(LogEventLevel.Error, message);
                    break;

                case SentinelLogLevel.Critical:
                    _logger.Write(LogEventLevel.Fatal, message);
                    break;

                default:
                    _logger.Write(LogEventLevel.Information, message);
                    break;
            }
        }
    }
}