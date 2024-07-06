using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Trg.Tech.Framework.Classes.Logging;
using System.Data.SqlClient;
using System.Collections;
using System.Diagnostics;
using System.IO;
using log4net.Config;
using log4net;
using log4net.Repository;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using log4net.Core;
using log4net.Filter;
using Insight360.Common;

namespace Insight360.Util
{
    public static class Log
    {
        static string GetMethodName()
        {
            var stackFrame = new StackTrace(true).GetFrame(2);
            var methodInfo = stackFrame.GetMethod();

            return methodInfo.DeclaringType.FullName
                + "."
                + methodInfo.Name
                + "-"
                + stackFrame.GetFileLineNumber()

                ;
        }

        static Log()
        {
            //XmlConfigurator.Configure();
        }

        //public static void WriteDBException(Exception ex, string query, List<SqlParameter> parameters)
        //{
        //    if (parameters == null)
        //        WriteDatabaseException(GetMethodName(), ex, query, (SqlParameter[])null);
        //    else
        //        WriteDatabaseException(GetMethodName(), ex, query, parameters.ToArray());
        //}

        //public static void WriteDBException(Exception ex, string query, SqlParameter[] parameters)
        //{
        //    WriteDatabaseException(GetMethodName(), ex, query, parameters);
        //}

        //static void WriteDatabaseException(string methodName, Exception ex, string query, SqlParameter[] parameters)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("Query: " + query);

        //    if (parameters != null)
        //    {
        //        for (int i = 0; i < parameters.Length; i++)
        //            sb.AppendLine("Parameter: " + parameters[i].ParameterName + " = " + parameters[i].Value);
        //    }
        //    sb.AppendLine("Exception: " + ex.ToString());
        //    ApplicationTrace.Log(methodName, sb.ToString(), Status.Failed);
        //}

        static string GetDbCommandText(SqlCommand cmd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Query: " + cmd.CommandText);

            if (cmd.Parameters != null)
            {
                for (int i = 0; i < cmd.Parameters.Count; i++)
                    sb.Append(" Parameter: " + cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value + " , ");
            }
            return sb.ToString();
        }
        public static void WriteDbException(Type sourceType, string sourceMethod, Exception exception, SqlCommand cmd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetDbCommandText(cmd));
            sb.Append(" Exception: " + exception.ToString());
            SetInfoFileAppender(sourceType, "Error");
            var logger = (Logger)LogManager.GetLogger(sourceType.Assembly.GetName().Name + "Error").Logger;
            logger.Level = Level.Error;
            LogManager.GetLogger(sourceType.Assembly.GetName().Name + "Error").Error(sourceMethod, exception);
        }

        public static void WriteException(Type sourceType, string sourceMethod, Exception exception)
        {
            SetInfoFileAppender(sourceType, "Error");
            var logger = (Logger)LogManager.GetLogger(sourceType.Assembly.GetName().Name + "Error").Logger;
            logger.Level = Level.Error;

            LogManager.GetLogger(sourceType.Assembly.GetName().Name + "Error").Error(sourceMethod, exception);
        }

        public static void WriteOperationStarted(Type sourceType, string sourceMethod, string message)
        {
            SetInfoFileAppender(sourceType, "Info");
            var logger = (Logger)LogManager.GetLogger(sourceType.Assembly.GetName().Name + "Info").Logger;
            logger.Level = Level.Info;
            logger.Parent = null;
            LogManager.GetLogger(sourceType.Assembly.GetName().Name + "Info").Info(sourceMethod + "-" + message + " Started");
        }

        public static void WriteOperationCompleted(Type sourceType, string sourceMethod, string message)
        {
            SetInfoFileAppender(sourceType, "Info");
            var logger = (Logger)LogManager.GetLogger(sourceType.Assembly.GetName().Name + "Info").Logger;
            logger.Level = Level.Info;
            LogManager.GetLogger(sourceType.Assembly.GetName().Name + "Info").Info(sourceMethod + "-" + message + " Completed");
        }

        public static void WriteDbCommand(Type sourceType, string sourceMethod, SqlCommand cmd)
        {
            SetInfoFileAppender(sourceType, "Info");
            var logger = LogManager.GetLogger(sourceType.Assembly.GetName().Name + "Info");
            logger.Info(sourceMethod + "-" + GetDbCommandText(cmd));
        }

        private static void SetInfoFileAppender(Type sourceType, string logType)
        {
            // get the current logging repository for this application

            ILoggerRepository repository = LogManager.GetRepository();
            //get all of the appenders for the repository 
            IAppender[] appenders = repository.GetAppenders();
            //only change the file path on the 'FileAppenders' 
            var appender = (from iAppender in appenders
                            where iAppender.Name == sourceType.Assembly.GetName().Name + logType
                            select iAppender).FirstOrDefault();

            if (appender == null)
            {
                MapAppender(sourceType, logType, Setup(sourceType, logType));
            }
        }

        public static RollingFileAppender Setup(Type sourceType, string logType)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%-5p %d %5rms %-22.22c{1} %-18.18M - %m%n";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.Name = sourceType.Assembly.GetName().Name + logType;
            roller.AppendToFile = true;
            roller.File = @"logs\" + sourceType.Assembly.GetName().Name + @"\" + logType + ".txt";
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 10;
            roller.MaximumFileSize = AppSettings.LogFileSize;
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = false;

            if (logType == "Error")
            {
                roller.Threshold = Level.Error;
                LevelRangeFilter lrf = new LevelRangeFilter();
                lrf.LevelMin = Level.Warn;
                lrf.LevelMax = Level.Fatal;
                roller.AddFilter(lrf);
                //hierarchy.Root.Level = Level.Error;
            }
            else
            {
                roller.Threshold = Level.Info;
                //hierarchy.Root.Level = Level.Info;
                LevelRangeFilter lrf = new LevelRangeFilter();
                lrf.LevelMin = Level.Info;
                lrf.LevelMax = Level.Info;
                roller.AddFilter(lrf);
            }

            roller.ActivateOptions();
            //hierarchy.Root.AddAppender(roller);
            hierarchy.Configured = true;
            return roller;
        }

        // Add an appender to a logger
        public static void MapAppender(Type sourceType, string logType, RollingFileAppender appender)
        {
            ILog log = LogManager.GetLogger(sourceType.Assembly.GetName().Name + logType);
            Logger l = (Logger)log.Logger;
            l.RemoveAllAppenders();
            l.AddAppender(appender);
        }

    }
}
