using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Diplo.TraceLogViewer.Models;
using Diplo.TraceLogViewer.Services;
using NUnit.Framework;

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
                @"E:\Diplo\Some Path\App_Data\Logs\UmbracoTraceLog.MictPHC124-PC.txt.2014-11-19",
                @"g:\iis-logs\logfiles\site-name\umbraco\Umbraco.2013-09-14.txt",
                @"x:\\some path\logs\UmbracoTraceLog.helloworld.2016-12-20.1.txt",
                @"x:\\some path\logs\UmbracoTraceLog.helloworld.2016-12-20.2.txt",
            };

            invalidDummyLogFileNames = new string[]
            {
                @"E:\Diplo\Some Path\App_Data\Logs\SomeOtherFile.txt",
                @"E:\Kittens\Some Path\App_Data\Logs\UmbracoTraceLog.txt.bak",
                @"E:\Diplo\Some Path\App_Data\Logs\UmbracoTraceLog.txt.2015-10-01.bak",
                @"E:\Diplo\Some Path\App_Data\Logs\UmbracoTraceLog.txt.2015-15-01",
                @"g:\iis-logs\logfiles\site-name\umbraco\Umbraco.2013-09-14.2013-09-14.txt"
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

            Assert.That(logItems.Count, Is.EqualTo(11));

            TestIsCorrectFormat(logItems[0], DateTime.Today, "UmbracoTraceLog.txt", null);

            TestIsCorrectFormat(logItems[1], DateTime.Parse("2016-12-20"), "UmbracoTraceLog.helloworld.2016-12-20.1.txt", "helloworld");

            TestIsCorrectFormat(logItems[3], DateTime.Parse("2016-01-16"), "UmbracoTraceLog.Machine.Name.txt.2016-01-16", "Machine.Name");

            TestIsCorrectFormat(logItems[8], DateTime.Parse("2015-09-05"), "UmbracoTraceLog.txt.2015-09-05", null);

            TestIsCorrectFormat(logItems[9], DateTime.Parse("2014-11-19"), "UmbracoTraceLog.MictPHC124-PC.txt.2014-11-19", "MictPHC124-PC");

            TestIsCorrectFormat(logItems[9], DateTime.Parse("2013-09-14"), "Umbraco.2013-09-14.txt", null);
        }

        private static void TestIsCorrectFormat(LogFileItem logItem, DateTime expectedDate, string expectedPath, string expectedMachineName)
        {
            TestContext.WriteLine("Testing: " + logItem);

            Assert.That(logItem.Date.Date == expectedDate.Date);
            Assert.That(Path.GetFileName(logItem.Path) == expectedPath);
            Assert.That(logItem.MachineName == "helloworld.2016-12-20.2.txt");
        }
    }
}
