using Diplo.TraceLogViewer.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Diplo.TraceLogViewer.Tests
{
    /// <summary>
    /// Tests for reading the log file names
    /// </summary>
    [TestFixture]
    public class LogFileServiceTests
    {
        private readonly List<string> allDummyLogFileNames;
        private readonly string[] validDummyLogFileNames;
        private readonly string[] invalidDummyLogFileNames;

        public LogFileServiceTests()
        {
            validDummyLogFileNames = new string[]
            {
                @"E:\Diplo\Some Path\App_Data\Logs\UmbracoTraceLog.txt",
                @"C:\Somepath\Donkey\UmbracoTraceLog.txt.2015-09-05",
                @"D:\Windblows\Logs\UmbracoTraceLog.txt.2015-10-06",
                @"E:\Diplo\Some Path\App_Data\Logs\umbracotracelog.txt.2016-01-02",
                @"F:\Kittens\Pictures\App_Data\Logs\UmbracoTraceLog.MachineName.txt.2016-01-04",
                @"E:\Diplo\Some Path\\Logs\UmbracoTraceLog.MachineName.TXT.2016-01-15",
                @"E:\Diplodocus\another\banana\App_Data\Logs\UmbracoTraceLog.Machine.Name.txt.2016-01-16",
                @"E:\Diplo\Some Path\App_Data\Logs\UmbracoTraceLog.MictPHC124-PC.txt.2014-11-19"
            };

            invalidDummyLogFileNames = new string[]
            {
                @"E:\Diplo\Some Path\App_Data\Logs\SomeOtherFile.txt",
                @"E:\Kittens\Some Path\App_Data\Logs\UmbracoTraceLog.txt.bak",
                @"E:\Diplo\Some Path\App_Data\Logs\UmbracoTraceLog.txt.2015-10-01.bak",
                @"E:\Diplo\Some Path\App_Data\Logs\UmbracoTraceLog.txt.2015-15-01"
            };

            allDummyLogFileNames = new List<string>(validDummyLogFileNames);
            allDummyLogFileNames.AddRange(invalidDummyLogFileNames);
        }

        /// <summary>
        /// Tests whether it parses the correct filenames and dates from log file paths and orders them correctly
        /// </summary>
        [Test]
        public void Has_Correct_Files()
        {
            LogFileService logFileService = new LogFileService();

            var logItems = logFileService.GetDateSortedLogFileDataFromFileNames(allDummyLogFileNames).ToArray();

            foreach (var logItem in logItems)
            {
                TestContext.WriteLine(logItem);
            }

            Assert.That(logItems.Count, Is.EqualTo(8));

            Assert.That(logItems[0].Date.Date == DateTime.Today);
            Assert.That(Path.GetFileName(logItems[0].Path) == "UmbracoTraceLog.txt");

            Assert.That(logItems[1].Date.Date == DateTime.Parse("2016-01-16"));
            Assert.That(Path.GetFileName(logItems[1].Path) == "UmbracoTraceLog.Machine.Name.txt.2016-01-16");

            Assert.That(logItems[6].Date.Date == DateTime.Parse("2015-09-05"));
            Assert.That(Path.GetFileName(logItems[6].Path) == "UmbracoTraceLog.txt.2015-09-05");

            Assert.That(logItems[7].Date.Date == DateTime.Parse("2014-11-19"));
            Assert.That(Path.GetFileName(logItems[7].Path) == "UmbracoTraceLog.MictPHC124-PC.txt.2014-11-19");

        }
    }
}
