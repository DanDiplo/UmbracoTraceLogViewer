using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private static readonly Regex LogEntryRegex = new Regex(CombinedLogEntryPattern, RegexOptions.Singleline | RegexOptions.Compiled);

        // Example: T123/D21
        // Example: Thread 123
        // Example: P3996/T48/D4
        private const string ThreadProcessPattern = @"T(?<THREAD>\d+)|D(?<DOMAIN>\d+)|P(?<PROCESS>\d+)|Thread (?<THREADOLD>\d+)";
        private static readonly Regex ThreadProcessRegex = new Regex(ThreadProcessPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Gets a collection of log file data items from a given log filename in the Umbraco log file folder
        /// </summary>
        /// <param name="fileName">The filename of the log file</param>
        /// <returns>An enumerable collection of log file data</returns>
        public IEnumerable<LogDataItem> GetLogDataFromDefaultFilePath(string fileName)
        {
            string logFilePath = Path.Combine(LogFileService.BaseLogPath, fileName);

            return GetLogDataFromFilePath(logFilePath);
        }
        
        /// <summary>
        /// Gets a collection of log data from a given location and reads and processes it entry-by-entry
        /// </summary>
        /// <param name="logFilePath">The full path to the log file</param>
        /// <returns>An enumerable collection of log file data</returns>
        public IEnumerable<LogDataItem> GetLogDataFromFilePath(string logFilePath)
        {
            if (File.Exists(logFilePath))
            {
                return ProcessLogStream(logFilePath);
            }
            else
            {
                throw new FileNotFoundException("The requested trace log file '" + Path.GetFileName(logFilePath) + "' could not be found", logFilePath);
            }
        }

        /// <summary>
        /// Experimental method to processe log data line-by-line
        /// </summary>
        /// <param name="fileLocation">The file path of the log gile</param>
        /// <returns>A collection of log data items</returns>
        public IEnumerable<LogDataItem> ProcessLogStream(string fileLocation)
        {
            return ProcessLog(File.OpenText(fileLocation));
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
        /// <param name="reader">The lines to process</param>
        /// <returns>A collection of log data items</returns>
        public IEnumerable<LogDataItem> ProcessLog(TextReader reader)
        {
            int id = 0;

            using (reader)
            {
                var logEntryLines = new List<LogDataItem>();
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var match = CheckIsLongEntryMatch(line);

                    if (match.Success)
                    {
                        if (logEntryLines.Count > 0)
                        {
                            yield return logEntryLines[0];
                            logEntryLines.Clear();
                        }

                        logEntryLines.Add(ParseLogDataItem(line, match, id++));
                    }
                    else
                    {
                        if (logEntryLines.Count > 0)
                        {
                            logEntryLines[0].Message += string.Concat("\n", line);
                        }
                    }
                }

                if (logEntryLines.Count > 0)
                {
                    yield return logEntryLines[0];
                    logEntryLines.Clear();
                }
            }
        }

        /// <summary>
        /// Parses the parts of each log data item
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <param name="match">The original log entry match</param>
        /// <returns>A LogDataItem that contains the individual log parts</returns>
        public static LogDataItem ParseLogDataItem(string line, Match match, int id = 0)
        {
            line = line.TrimStart();

            // 2016-05-02 19:43:39,042
            var date = DateTime.ParseExact(line.Substring(0, 23), "yyyy-MM-dd HH:mm:ss,FFF", CultureInfo.InvariantCulture);

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
                Id = id,
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

        /// <summary>
        /// Gets the time and date, as ticks, when the file was last modified
        /// </summary>
        /// <param name="fileName">The filename to check</param>
        /// <returns>The number of ticks</returns>
        public long GetLastModifiedTicks(string fileName)
        {
            string logFilePath = Path.Combine(LogFileService.BaseLogPath, fileName);

            System.IO.FileInfo fi = new FileInfo(logFilePath);
            return fi.LastWriteTimeUtc.Ticks;
        }
    }

}
