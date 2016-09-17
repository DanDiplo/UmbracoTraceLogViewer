using System;
using System.Collections.Generic;
using System.Linq;
using Diplo.TraceLogViewer.Models;
using Diplo.TraceLogViewer.Services;
using Umbraco.Core;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Diplo.TraceLogViewer.Controllers
{
    /// <summary>
    /// API Controller for accessing Umbraco trace log data - accessible to Umbraco developers
    /// </summary>
    [UmbracoApplicationAuthorize(Constants.Applications.Developer)]
    [PluginController("TraceLogViewer")]
	public class TraceLogController : UmbracoAuthorizedJsonController
	{
        private LogDataService logDataService;
        private LogFileService logFileService;

        /// <summary>
        /// Instantiate a new instance with the default log services
        /// </summary>
        public TraceLogController() : this(new LogDataService(), new LogFileService())
        {
        }

        /// <summary>
        /// Instantiates a new instance with the passed in service implementations
        /// </summary>
        /// <param name="logDataService">A log data service</param>
        /// <param name="logFileService">A log file service</param>
        public TraceLogController(LogDataService logDataService, LogFileService logFileService)
        {
            this.logDataService = logDataService;
            this.logFileService = logFileService;
        }

		/// <summary>
		/// Gets a list of trace log files from the default location in /App_Data/Logs/
		/// </summary>
		/// <returns>A collection of log file items</returns>
		/// <remarks>/Umbraco/TraceLogViewer/TraceLog/GetLogFilesList</remarks>
		public IEnumerable<LogFileItem> GetLogFilesList()
		{
			return logFileService.GetLogFiles();
		}

        /// <summary>
        /// Gets the trace log data and metadata for the file with a given filename
        /// </summary>
        /// <returns>Log data for the log file</returns>
        /// <remarks>/Umbraco/TraceLogViewer/TraceLog/GetLogDataResponse</remarks>
        public LogDataResponse GetLogDataResponse(string logfileName)
        {
            return new LogDataResponse()
            {
                LogDataItems = this.logDataService.GetLogDataFromDefaultFilePath(logfileName).OrderByDescending(l => l.Date),
                LastModifiedTicks = this.logDataService.GetLastModifiedTicks(logfileName)
            };
        }

        /// <summary>
        /// Gets the trace log data for the file with a given filename
        /// </summary>
        /// <returns>Log data for the log file</returns>
        /// <remarks>/Umbraco/TraceLogViewer/TraceLog/GetLogData</remarks>
        public IEnumerable<LogDataItem> GetLogData(string logfileName)
		{
			return this.logDataService.GetLogDataFromDefaultFilePath(logfileName).OrderByDescending(l => l.Date);
		}

        /// <summary>
        /// Gets the time (in ticks) when the given log file was last modified
        /// </summary>
        /// <param name="logfileName">The log file to check</param>
        /// <returns>The time and date in ticks</returns>
        public long GetLastModifiedTime(string logfileName)
        {
            return this.logDataService.GetLastModifiedTicks(logfileName);
        }
    }
}
