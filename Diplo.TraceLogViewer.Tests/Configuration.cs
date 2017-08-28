using System;
using System.IO;

namespace Diplo.TraceLogViewer.Tests
{
    /// <summary>
    /// Configuration values for Unit tests
    /// </summary>
    public static class Configuration
    {
        private const string logFilesPath = @"..\..\TestLogFiles\"; // relative to bin folder

        public static readonly string TestLogsDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFilesPath));

        public const string UmbracoFile = "UmbracoTraceLog.txt";

        public const string UmbracoBigFile = "UmbracoTraceLog.MictPHC124-PC.txt";
    }
}
