using System;

namespace Diplo.TraceLogViewer.Models
{
    /// <summary>
    /// Represents a single row of data in an Umbraco trace log file
    /// </summary>
    public class LogDataItem
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Level { get; set; }

        public string Logger { get; set; }

        public string Message { get; set; }

        public string ThreadId { get; set; }

        public string DomainId { get; set; }

        public string ProcessId { get; set; }

        public override string ToString()
        {
            return String.Format("Date: {0}, Level: {1}, Logger: {2}, ThreadId: {3}, DomainId: {4}, ProcessId: {5}, Message: {6}",
                this.Date, this.Level, this.Logger, this.ThreadId, this.DomainId, this.ProcessId, this.Message);
        }
    }
}
