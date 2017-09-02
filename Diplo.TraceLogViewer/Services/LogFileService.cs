using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using Diplo.TraceLogViewer.Models;
using log4net.Appender;
using Umbraco.Core;

namespace Diplo.TraceLogViewer.Services
{
    /// <summary>
    /// Service for getting log file names and dates from a directory
    /// </summary>
    public class LogFileService
    {
        private const string dateFormat = @"\d{4}-\d{2}-\d{2}";
        private const string defaultLogPath = "~/App_Data/Logs/";
        private const string defautlLogFnPattern = "Umbraco(TraceLog)?";

        private static readonly Regex datePatternRegex = new Regex(dateFormat);
        private static string baseLogPath;
        private static string baseLogFilename;

        /// <summary>
        /// Gets the absolute path to the folder where the logs are stored
        /// </summary>
        public static string BaseLogPath
        {
            get
            {
                return baseLogPath ?? (baseLogPath = ResolveBaseLogPath());
            }
        }

        public static string BaseLogFilename
        {
            get
            {
                return baseLogFilename ?? (baseLogFilename = ResolveBaseLogFileName());
            }
        }

        /// <summary>
        /// Gets the log files from the default log file directory
        /// </summary>
        /// <returns>A collection of log file items</returns>
        public IEnumerable<LogFileItem> GetLogFiles()
        {
            return GetLogFilesFromPath(BaseLogPath);
        }

        /// <summary>
        /// Gets the log files from a given path
        /// </summary>
        /// <param name="fullPath">The full path to the log file directory</param>
        /// <returns>A collection of log file items</returns>
        public IEnumerable<LogFileItem> GetLogFilesFromPath(string fullPath)
        {
            var filenames = Directory.GetFiles(fullPath);
            return GetDateSortedLogFileDataFromFileNames(filenames);
        }

        /// <summary>
        /// Gets the log file names and dates from the collection of filenames and sorts them into date order
        /// </summary>
        /// <param name="filenames">The file names to process</param>
        /// <returns>A sorted list of log file items</returns>
        public IEnumerable<LogFileItem> GetDateSortedLogFileDataFromFileNames(IEnumerable<string> filenames)
        {
            if (filenames == null)
            {
                throw new ArgumentNullException("filenames");
            };

            var files = new List<LogFileItem>();

            foreach (var filePath in filenames)
            {
                DateTime logDate = GetLogFileDate(filePath);
                files.Add(new LogFileItem(logDate.Date, filePath, null));

            }

            var sortedFiles = files.OrderByDescending(x => x.Date);

            return sortedFiles;
        }

        public static DateTime GetLogFileDate(string filePath)
        {
            string fileName = System.IO.Path.GetFileName(filePath);
            var logDate = DateTime.Today;

            var dateMatch = datePatternRegex.Match(fileName);

            if (dateMatch.Success)
            {
                string datePattern = dateMatch.Value;

                if (!DateTime.TryParse(datePattern, out logDate))
                {
                    logDate = File.GetLastWriteTime(filePath);
                }
            }
            else
            {
                logDate = File.GetLastWriteTime(filePath);
            }

            return logDate;
        }

        private static string ResolveBaseLogFileName()
        {
            var loggerRepo = log4net.LogManager.GetRepository();

            if (loggerRepo != null)
            {
                var appender = loggerRepo.GetAppenders().FirstOrDefault(a => "rollingFile".InvariantEquals(a.Name)) as RollingFileAppender;

                if (appender != null)
                {
                    var fn = Path.GetFileName(appender.File);
                    return fn.Split('.')[0];
                }
            }

            return defautlLogFnPattern;
        }

        /// <summary>
        /// Resolve the base log path, based on the log4net configured appenders.
        /// </summary>
        /// <returns>The absolute path</returns>
        private static string ResolveBaseLogPath()
        {
            var loggerRepo = log4net.LogManager.GetRepository();

            if (loggerRepo != null)
            {
                var appender = loggerRepo.GetAppenders().FirstOrDefault(a => "rollingFile".InvariantEquals(a.Name)) as RollingFileAppender;

                if (appender != null)
                {
                    return Path.GetDirectoryName(appender.File);
                }
            }

            return HostingEnvironment.MapPath(defaultLogPath);
        }
    }
}
