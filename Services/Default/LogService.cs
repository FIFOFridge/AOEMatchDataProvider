﻿using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services.Default
{
    public sealed class LogService : ILogService, IDisposable
    {
        Logger logger; 

        public LogService(IAppConfigurationService appConfigurationService)
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logFileName = "log_" + DateTime.Now.ToString("d_MMM_yyyy-h_mm--ss", DateTimeFormatInfo.InvariantInfo);
                //.Replace(":", "-");

#if DEBUG
            //keep only single log file for each app session
            //var logFilePath = Path.Combine(appConfigurationService.StorageDirectory, "logs");
            var logFilePath = Path.Combine(appConfigurationService.StorageDirectory, "logs", $"_debug_log.txt");

            if (File.Exists(logFilePath))
            {
                //supress
                //    try
                //    {
                           File.Delete(logFilePath);
                //    }
                //    catch(Exception e)
                //    {
                //        if (
                //            e is StackOverflowException ||
                //            e is ThreadAbortException ||
                //            e is AccessViolationException
                //            )
                //        {
                //            throw e;
                //        }
                //    }
                //}
            }
#else //RELEASE
            var logFilePath = Path.Combine(appConfigurationService.StorageDirectory, "logs", $"{logFileName}.txt");
#endif
            // Targets where to log to: File and Console
            var logFile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = logFilePath,
                Layout = "[${level:uppercase=true}] ${longdate} Thread: ${threadid}: ${exception:format=toString} ${message}",
                CreateDirs = true
            };

#if DEBUG
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);
#else //RELEASE
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logFile);
#endif

            LogManager.Configuration = config;
            logger = LogManager.GetCurrentClassLogger();
        }

        public void Debug(string message, Dictionary<string, object> logProperties = null)
        {
            if (logProperties == null)
            {
                logger.Log(CreateLogEvent(LogLevel.Debug, message, logProperties));
                return;
            }

            logger.Debug(message);
        }

        public void Critical(string message, Dictionary<string, object> logProperties = null)
        {
            if(logProperties == null)
            {
                logger.Log(CreateLogEvent(LogLevel.Fatal, message, logProperties));
                return;
            }

            logger.Fatal(message);
        }

        public void Error(string message, Dictionary<string, object> logProperties = null)
        {
            if (logProperties == null)
            {
                logger.Log(CreateLogEvent(LogLevel.Error, message, logProperties));
                return;
            }

            logger.Error(message);
        }

        //public void Exception(string message, Exception exception, Dictionary<string, object> logProperties = null)
        //{
        //    if (logProperties == null)
        //    {
        //        logger.Log(CreateLogEvent(LogLevel.Error, message, logProperties));
        //        return;
        //    }

        //    logger.Log(LogLevel.Error, message, exception);
        //}

        public void Info(string message, Dictionary<string, object> logProperties = null)
        {
            if (logProperties == null)
            {
                logger.Log(CreateLogEvent(LogLevel.Info, message, logProperties));
                return;
            }

            logger.Info(message);
        }

        public void Warning(string message, Dictionary<string, object> logProperties = null)
        {
            logger.Warn(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        LogEventInfo CreateLogEvent(LogLevel logLevel, string message, Dictionary<string, object> logProperties)
        {
            var @event = new LogEventInfo
            {
                Message = message,
                Level = logLevel
            };

            if (logProperties != null)
            {
                foreach (var logProperty in logProperties)
                {
                    @event.Properties.Add(logProperty.Key, logProperty.Value);
                }
            }

            return @event;
        }

        public void Dispose()
        {
            if(logger != null)
                LogManager.Shutdown();
        }
    }
}