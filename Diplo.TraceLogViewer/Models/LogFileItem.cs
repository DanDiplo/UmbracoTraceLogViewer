using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplo.TraceLogViewer.Models
{
	/// <summary>
	/// Represents a log file item
	/// </summary>
	[Serializable]
	public class LogFileItem
	{
		/// <summary>
		/// Get the date of the log file
		/// </summary>
		public DateTime Date { get; private set; }

		/// <summary>
		/// Get the path to the log file
		/// </summary>
		public string Path { get; private set; }

		public LogFileItem(DateTime date, string path)
		{
			this.Date = date;
			this.Path = path;
		}
	}
}
