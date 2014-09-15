using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using Diplo.TraceLogViewer.Models;

namespace Diplo.TraceLogViewer.Services
{
	/// <summary>
	/// Used to query trace log data from trace log files
	/// </summary>
	/// <remarks>
	/// Example entry: 2014-05-26 15:48:51,750 [5] INFO  Umbraco.Core.PluginManager - [Thread 1] Determining hash of code files on disk
	/// </remarks>
	public class LogDataService
	{
		//Example: 2014-05-26 15:48:51,750 [5] INFO  Umbraco.Core.PluginManager - [Thread 1] Determining hash of code files on disk
		private const string LogEntryPattern = @"((\d{4}-\d{2}-\d{2}) (\d{2}:\d{2}:\d{2},\d{3}) (\[(\d+)\]) (\w+) {1,2}(.+) - (.+))";
		private static readonly Regex LogEntryRegex = new Regex(LogEntryPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

		//Example: 2014-05-26 15:48:51,750 [5] INFO  Umbraco.Core.PluginManager - [Thread 1]
		//The last group in the match will have the thread number
		private const string ThreadNumberPattern = @"((.+) (\[\d+\]) (.+) (\[Thread (\d+)\] ?))";
		private static readonly Regex ThreadNumberRegex = new Regex(ThreadNumberPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

		/// <summary>
		/// Gets a collection of log file data items from a given log filename
		/// </summary>
		/// <param name="fileName">The filename of the log file</param>
		/// <returns>An enumerable collection of log file data</returns>
		public IEnumerable<LogDataItem> GetLogDataFromFile(string fileName)
		{
			string fullPath = HostingEnvironment.MapPath(Path.Combine(LogFileService.BaseLogPath, fileName));

			return GetLogData(fullPath);
		}

		/// <summary>
		/// Gets a collection of log file data items from a given filepath to a log file
		/// </summary>
		/// <param name="logFilePath">The full file path to the log file</param>
		/// <returns>An enumerable collection of log file data</returns>
		public IEnumerable<LogDataItem> GetLogData(string logFilePath)
		{
			var logItems = new List<LogDataItem>();

			if (File.Exists(logFilePath))
			{
				string log = File.ReadAllText(logFilePath, Encoding.UTF8);

				var allLines = log.Split('\n');

				foreach (var line in allLines)
				{
					var match = LogEntryRegex.Match(line);
					if (match.Success)
					{
						var date = DateTime.Parse(line.Substring(0, 19));

						var logDataItem = new LogDataItem
						{
							Date = date,
							Level = match.Groups[6].Value,
							Logger = match.Groups[7].Value,
							Message = match.Groups[8].Value,
							ThreadId = match.Groups[5].Value,
						};

						// Some log messages have [Thread x], others don't, use the data when available
						var threadMatch = ThreadNumberRegex.Match(line);
						if (threadMatch.Success)
						{
							logDataItem.ThreadNo = threadMatch.Groups[6].Value;

							// Remove the [Thread x] message from the message, it's duplicate info
							logDataItem.Message = logDataItem.Message.Replace(threadMatch.Groups[5].Value, string.Empty);
						}

						logItems.Add(logDataItem);
					}
					else
					{
						if (logItems.Any())
							logItems.Last().Message = logItems.Last().Message + "\n" + line;
					}
				}
			}
			else
			{
				throw new FileNotFoundException("The requested trace log file '" + logFilePath + "' could not be found", logFilePath);
			}

			return logItems;
		}
	}
}
