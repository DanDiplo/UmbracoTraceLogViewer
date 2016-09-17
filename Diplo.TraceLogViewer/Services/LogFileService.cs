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
        private const string dateFormat = @"(?<date>\d{4}-\d{2}-\d{2})";
        private const string defaultLogPath = "~/App_Data/Logs/";
        private const string defautlLogFnPattern = "Umbraco(TraceLog)?";

        private string filePattern; // matches valid log file name      
        private static string datePattern; // matches date pattern in log file name
        private static string machinePattern;


        private readonly Regex filePatternRegex;
        private static string baseLogPath;
        private static string baseLogFilename;

        public LogFileService()
        {
            datePattern = @"((" + dateFormat + ".txt)$|(txt." + dateFormat + ")$)";
            machinePattern = @"(?<machine>((?!" + dateFormat + @").*))";
            filePattern = @"(?<path>.*)" +
                          @"(?<file>" + BaseLogFilename + @")\." +
                          @"(" + machinePattern + @"\.)?" +
                          @"(" + datePattern + "|txt$)";

            filePatternRegex = new Regex(filePattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        }

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
            var filenames = Directory.GetFiles(fullPath, BaseLogFilename + ".*");
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

            foreach (var f in filenames)
            {
                string machineName = null;

                Match fileMatch = filePatternRegex.Match(f);

                if (fileMatch.Success)
                {
                    var logDate = DateTime.Now;
                    var date = fileMatch.Groups["date"].Value;

                    if (!string.IsNullOrWhiteSpace(date) && !DateTime.TryParse(date, out logDate))
                    {
                        continue;
                    }

                    var machineGroup = fileMatch.Groups["machine"].Value;
                    machineName = string.IsNullOrWhiteSpace(machineGroup) ? null : machineGroup;
                    files.Add(new LogFileItem(logDate.Date, f, machineName));
                }
            }

            var sortedFiles = files.OrderByDescending(x => x.Date);

            return sortedFiles;
        }
    }
}
