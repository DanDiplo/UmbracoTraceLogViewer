using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
		//2013-06-15 20:38:21,691
		const string LogDatePattern = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3} )";
		private static Regex logDateRegex = new Regex(LogDatePattern, RegexOptions.Compiled);

		//[5] INFO  Umbraco.Core.PluginManager - [Thread 1] Determining hash of code files on disk
		const string LogEntryPattern = @"^.+(\[\d+\]) (\w+) {1,2}(.+) - (\[Thread \d+\]) (.+)";
		private static Regex logEntryRegex = new Regex(LogEntryPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
				string log = File.ReadAllText(logFilePath);

				var matches = logDateRegex.Split(log);

				var entries = new List<string>();

				for (int i = 1; i < matches.Length - 1; i = i + 2)
				{
					entries.Add(matches[i] + matches[i + 1]);
				}

				foreach (var entry in entries)
				{
					var item = new LogDataItem();

					item.Date = DateTime.Parse(entry.Substring(0, 19));
					item.Level = entry;
					logItems.Add(item);

					Match match = logEntryRegex.Match(entry);

					if (match.Success)
					{
						item.ThreadNo = match.Groups[1].Value;
						item.Level = match.Groups[2].Value;
						item.Logger = match.Groups[3].Value;
						item.ThreadId = match.Groups[4].Value;
						item.Message = match.Groups[5].Value;
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
