using System;

namespace Diplo.TraceLogViewer.Models
{
    /// <summary>
    /// Represents a log file item (date and path)
    /// </summary>
    public class LogFileItem
    {
        private LogFileItem()
        {
            this.MachineName = string.Empty;
        }

        /// <summary>
        /// Instantiate a new log item from the date and path
        /// </summary>
        /// <param name="date">The log file date</param>
        /// <param name="path">The log file relative path</param>
        public LogFileItem(DateTime date, string path) : this()
        {
            this.Date = date;
            this.Path = path;
        }

        /// <summary>
        /// Get the date of the log file
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Get the full path to the log file
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets just the file name of the log file
        /// </summary>
        public string Filename => System.IO.Path.GetFileName(this.Path);

        /// <summary>
        /// Gets the machine name part of the log file
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets whether this is a Courier log
        /// </summary>
        public bool IsCourier { get; set; }

        public override string ToString()
        {
            return string.Format("{0:yyyy-MM-dd} - [{1}]", this.Date, this.Filename);
        }
    }
}
