using NLog;
using Services.Interfaces;

namespace Services.Implementation
{
    public class LoggerManager : ILoggerManager
    {
        private static NLog.ILogger logger = LogManager.GetCurrentClassLogger();

        public LoggerManager() { }

        public void LogDebug(string message)
        {
            logger.Debug(message);
        }

        public void LogError(Exception ex)
        {
            logger.Error(ex.Message);
        }

        public void LogInfo(string message)
        {
            logger.Info(message);
        }

        public void LogWarn(string message)
        {
            logger.Warn(message);
        }
    }
}
