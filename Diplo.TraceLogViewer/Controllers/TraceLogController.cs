using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.WebApi;
using Diplo.TraceLogViewer.Models;
using Diplo.TraceLogViewer.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Diplo.TraceLogViewer.Controllers
{
	/// <summary>
	/// Controller for accessing Umbraco trace log data
	/// </summary>
    [UmbracoApplicationAuthorize("developer")]
	[PluginController("TraceLogViewer")]
	public class TraceLogController : UmbracoAuthorizedApiController
	{
		/// <summary>
		/// Gets a list of trace log files
		/// </summary>
		/// <returns>A collection of log file items</returns>
		/// <remarks>/Umbraco/TraceLogViewer/TraceLog/GetLogFilesList</remarks>
		public IEnumerable<LogFileItem> GetLogFilesList()
		{
			LogFileService service = new LogFileService();
			return service.GetLogFiles();
		}

		/// <summary>
		/// Gets the trace log data for the file with a given filename
		/// </summary>
		/// <returns>Log data for the log file</returns>
		/// <remarks>/Umbraco/TraceLogViewer/TraceLog/GetLogData</remarks>
		public IEnumerable<LogDataItem> GetLogData(string logfileName)
		{
			LogDataService service = new LogDataService();

			return service.GetLogDataFromFile(logfileName);
		}
	}
}
