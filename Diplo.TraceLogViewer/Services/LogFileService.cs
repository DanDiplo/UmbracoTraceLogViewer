using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using Diplo.TraceLogViewer.Models;
using Umbraco.Core;
using log4net.Appender;

namespace Diplo.TraceLogViewer.Services
{
    /// <summary>
    /// Service for getting log file names and dates from a directory
    /// </summary>
    public class LogFileService
    {
        private const string filePattern = @".*UmbracoTraceLog.*(\.txt|\d{4}-\d{2}-\d{2})$"; // matches valid log file name
        private const string datePattern = @".txt.(\d{4}-\d{2}-\d{2})"; // matches date pattern in log file name
        private const string defaultLogPath = "~/App_Data/Logs/";


        private static Regex filePatternRegex = new Regex(filePattern, RegexOptions.IgnoreCase);
        private static Regex datePatternRegex = new Regex(datePattern, RegexOptions.IgnoreCase);

        private static string baseLogPath;

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
            var filenames = Directory.GetFiles(fullPath, "UmbracoTraceLog.*");
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
                Match fileMatch = filePatternRegex.Match(f);

                if (fileMatch.Success)
                {
                    var logDate = DateTime.Now;

                    Match dateMatch = datePatternRegex.Match(f);

                    if (dateMatch.Success && dateMatch.Groups.Count > 0)
                    {
                        if (!DateTime.TryParse(dateMatch.Groups[1].Value, out logDate))
                        {
                            continue;
                        }
                    }

                    files.Add(new LogFileItem(logDate.Date, f));
                }
            }

            var sortedFiles = files.OrderByDescending(x => x.Date);

            return sortedFiles;
        }
    }
}
