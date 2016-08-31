using System;
using System.Collections.Generic;
using System.Linq;

namespace Diplo.TraceLogViewer.Models
{
    /// <summary>
    /// Represents a response for a GetLogDataResponse request
    /// </summary>
    public class LogDataResponse
    {
        /// <summary>
        /// Get the log file items
        /// </summary>
        public IEnumerable<LogDataItem> LogDataItems { get; set; }

        /// <summary>
        /// Get the time in ticks the log file was last modified
        /// </summary>
        public long LastModifiedTicks { get; set; }
    }
}
