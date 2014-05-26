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
	public class LogFileService
	{
		public static string BaseLogPath 
		{ 
			get { return "~/App_Data/Logs/"; }
		}

		const string datePattern = @".txt.(\d{4}-\d{2}-\d{2})";

		public IEnumerable<LogFileItem> GetLogFiles()
		{
			string fullPath = HostingEnvironment.MapPath(BaseLogPath);

			var files = new List<LogFileItem>();

			var filenames = Directory.GetFiles(fullPath, "UmbracoTraceLog.*");

			foreach (var f in filenames)
			{
				var logDate = DateTime.Now;

				Match match = Regex.Match(f, datePattern);

				if (match.Success)
				{
					logDate = DateTime.Parse(match.Groups[1].Value);
				}


				files.Add(new LogFileItem(logDate.Date, f));
			}

			var sortedFiles = files.OrderByDescending(x => x.Date);

			return sortedFiles;
		}
	}
}
