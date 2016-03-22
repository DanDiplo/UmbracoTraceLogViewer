using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diplo.TraceLogViewer.Services;
using NUnit.Framework;

namespace Diplo.TraceLogViewer.Tests
{
    [TestFixture]
    public class ProfilingTests
    {
        /// <summary>
        /// This method can be used to perform profiling tests. Output is shown in NUnit Output window.
        /// </summary>
        [Test]
        public void LogFile_Parsing_Performance()
        {
            const int Iterations = 1; // how many times we run parse the file

            TimeSpan totalStream = TimeSpan.Zero;
            int countStream = 0;
            Stopwatch sw = new Stopwatch();

            var file = Configuration.UmbracoBigFile; // the big file to use

            /* This uses the stream version that reads one entry at a time */
            for (int i = 0; i < Iterations; i++)
            {
                sw.Reset();
                sw.Start();

                var logFile = Path.Combine(Configuration.TestLogsDirectory, file);

                LogDataService dataService = new LogDataService();

                countStream = dataService.GetLogDataFromFilePath(logFile)
                    //It's IEnumerable = lazy executed so we need to iterate it
                    .Count();
                sw.Stop();

                totalStream = totalStream.Add(sw.Elapsed);
            }

            TestContext.WriteLine("Elapsed Time Stream: {0}\n", totalStream);
        }
    }
}
