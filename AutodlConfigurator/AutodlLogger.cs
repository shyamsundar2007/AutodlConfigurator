using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace AutodlConfigurator
{
    enum AutodlLogLevel
    {
        INFO = 0,
        DEBUG,
        WARNING,
        ERROR
    };

    static class AutodlLogger
    {
        /// <summary>
        /// Logger.
        /// </summary>
        static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Log(AutodlLogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case AutodlLogLevel.INFO: logger.Info(message);
                    break;
                case AutodlLogLevel.WARNING: logger.Warn(message);
                    break;
                case AutodlLogLevel.DEBUG: logger.Debug(message);
                    break;
                case AutodlLogLevel.ERROR: logger.Error(message);
                    break;
                default:
                    break;
            }
        }
    }
}
