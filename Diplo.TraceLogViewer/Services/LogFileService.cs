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
        private const string dateFormat = @"\d{4}-\d{2}-\d{2}"; // date matching regex
        private const string defaultLogPath = "~/App_Data/Logs/"; // default Umbraco log path
        private const string defautlLogFnPattern = "Umbraco(TraceLog)?"; // default Umbraco log pattern
        public const string machineNamePattern = ".+TraceLog.(.+).txt"; // machine name matching regex

        private static readonly Regex datePatternRegex = new Regex(dateFormat);
        private static readonly Regex machineNameRegex = new Regex(machineNamePattern);
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
                string fileName = System.IO.Path.GetFileName(filePath);
                DateTime logDate = GetLogFileDate(filePath);

                var logFileItem = new LogFileItem(logDate.Date, filePath)
                {
                    IsCourier = IsCourierLog(fileName),
                    MachineName = GetMachineName(fileName)
                };

                files.Add(logFileItem);
            }

            var sortedFiles = files.OrderByDescending(x => x.Date);

            return sortedFiles;
        }

        /// <summary>
        /// Gets the machine name component from the log filename
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The machine name</returns>
        public static string GetMachineName(string fileName)
        {
            var match = machineNameRegex.Match(fileName);

            string marchineName = String.Empty;

            if (match.Success && match.Groups.Count > 0)
            {
                marchineName = match.Groups[1].Value;
            }

            return marchineName;
        }

        /// <summary>
        /// Gets whether the log file is a Courier log
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>True if it is a Courier log; otherwise false</returns>
        public static bool IsCourierLog(string fileName)
        {
            return fileName.StartsWith("Courier", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the date from the log file
        /// </summary>
        /// <param name="filePath">The full path of the file</param>
        /// <returns>The date</returns>
        /// <remarks>If it can't be parsed from the filename uses the last-modified datestamp</remarks>
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
