using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using Diplo.TraceLogViewer.Models;

namespace Diplo.TraceLogViewer.Services
{
    /// <summary>
    /// Used to parse trace log data from Umbraco trace log files
    /// </summary>
    /// <remarks>
    /// Should now work with old log format and new 7.3.x format
    /// Try http://regexstorm.net/tester for testing
    /// </remarks>
    public class LogDataService
    {
        // Example: 2015-07-15 21:58:59,748 [P22252/D3/T67] INFO umbraco.BusinessLogic.Log - Latest Tweets error
        // Example: 2015-07-22 20:17:16,194 [8] INFO Umbraco.Core.CoreBootManager - [Thread 1] Umbraco 7.2.8 application starting on SPIRIT
        // Example: 2015-07-22 19:17:53,450 [8] INFO ProductCreationService - [P4812/T1/D2] Product Import. Finished CreateProducts - there were 0 errors.
        private const string CombinedLogEntryPattern = @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}\s(\[(?<PROCESS1>.+)\]|\s) (?<LEVEL>\w+) {1,5}(?<LOGGER>.+?) -(\s\[(?<PROCESS2>[A-Z]\d{1,6}/[A-Z]\d{1,6}/[A-Z]\d{1,6}|Thread \d.?)\]\s|\s)(?<MESSAGE>.+)";
        private static readonly Regex LogEntryRegex = new Regex(CombinedLogEntryPattern, RegexOptions.Singleline);

        // Example: T123/D21
        // Example: Thread 123
        // Example: P3996/T48/D4
        private const string ThreadProcessPattern = @"T(?<THREAD>\d+)|D(?<DOMAIN>\d+)|P(?<PROCESS>\d+)|Thread (?<THREADOLD>\d+)";
        private static readonly Regex ThreadProcessRegex = new Regex(ThreadProcessPattern, RegexOptions.IgnoreCase);

        /// <summary>
        /// Gets a collection of log file data items from a given log filename in the Umbraco log file folder
        /// </summary>
        /// <param name="fileName">The filename of the log file</param>
        /// <returns>An enumerable collection of log file data</returns>
        public IEnumerable<LogDataItem> GetLogDataFromDefaultFilePath(string fileName)
        {
            string logFilePath = HostingEnvironment.MapPath(Path.Combine(LogFileService.BaseLogPath, fileName));

            return GetLogDataFromFilePath(logFilePath);
        }

        /// <summary>
        /// Gets a collection of log file data from a given location path
        /// </summary>
        /// <param name="logFilePath">The full path to the log file</param>
        /// <returns>An enumerable collection of log file data</returns>
        public IEnumerable<LogDataItem> GetLogDataFromFilePath(string logFilePath)
        {
            if (File.Exists(logFilePath))
            {
                string log = File.ReadAllText(logFilePath);

                var allLines = log.Split('\n');

                return ProcessLog(allLines);
            }
            else
            {
                throw new FileNotFoundException("The requested trace log file '" + Path.GetFileName(logFilePath) + "' could not be found", logFilePath);
            }
        }

        /// <summary>
        /// Checks whether the given line matches a log file entry
        /// </summary>
        /// <param name="line">The line to check</param>
        /// <returns>A match object</returns>
        public static Match CheckIsLongEntryMatch(string line)
        {
            return LogEntryRegex.Match(line);
        }

        /// <summary>
        /// Processes a collection of log file lines and attempts to convert each entry to a LogDataItem
        /// </summary>
        /// <param name="allLines">The lines to process</param>
        /// <returns>A collection of log data items</returns>
        public IList<LogDataItem> ProcessLog(IEnumerable<string> allLines)
        {
            var logItems = new List<LogDataItem>();

            foreach (var line in allLines)
            {
                var match = CheckIsLongEntryMatch(line);

                if (match.Success)
                {
                    LogDataItem logDataItem = ParseLogDataItem(line, match);

                    logItems.Add(logDataItem);
                }
                else
                {
                    if (logItems.Any())
                    {
                        logItems.Last().Message = logItems.Last().Message + "\n" + line;
                    }
                }
            }

            return logItems;
        }

        /// <summary>
        /// Parses the parts of each log data item
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <param name="match">The original log entry match</param>
        /// <returns>A LogDataItem that contains the individual log parts</returns>
        public static LogDataItem ParseLogDataItem(string line, Match match)
        {
            line = line.TrimStart();

            var date = DateTime.Parse(line.Substring(0, 19));

            string threadProcess = match.Groups["PROCESS2"].Value;

            if (String.IsNullOrEmpty(threadProcess))
            {
                threadProcess = match.Groups["PROCESS1"].Value;
            }

            string threadId = null;
            string processId = null;
            string domainId = null;

            if (!String.IsNullOrEmpty(threadProcess))
            {
                var procMatches = ThreadProcessRegex.Matches(threadProcess);

                foreach (Match procMatch in procMatches)
                {
                    if (procMatch.Success)
                    {
                        var grp = procMatch.Groups["THREAD"];
                        if (grp.Success)
                        {
                            threadId = grp.Value;
                        }

                        grp = procMatch.Groups["PROCESS"];
                        if (grp.Success)
                        {
                            processId = grp.Value;
                        }

                        grp = procMatch.Groups["DOMAIN"];
                        if (grp.Success)
                        {
                            domainId = grp.Value;
                        }

                        if (threadId == null)
                        {
                            grp = procMatch.Groups["THREADOLD"];
                            if (grp.Success)
                            {
                                threadId = grp.Value;
                            }
                        }
                    }
                }
            }

            var logDataItem = new LogDataItem
            {
                Date = date,
                Level = match.Groups["LEVEL"].Value,
                Logger = match.Groups["LOGGER"].Value,
                Message = match.Groups["MESSAGE"].Value,
                DomainId = domainId,
                ProcessId = processId,
                ThreadId = threadId
            };

            return logDataItem;
        }
    }
}
