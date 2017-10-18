using System;
using System.Collections.Generic;
using Diplo.TraceLogViewer.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Diplo.TraceLogViewer.Tests
{
    [TestClass]
    public class LogNameTests
    {
        private static Dictionary<string, string> logFileNames = new Dictionary<string, string>
        {
            { "UmbracoTraceLog.SuperKitten-PC.txt", "SuperKitten-PC" },
            { @"C:\Somewhere\Courier\logs\CourierTraceLog.SuperKitten-PC.txt", "SuperKitten-PC" },
            { @"D:\somepath\logs\UmbracoTraceLog.BananaMan.txt.2017-09-15", "BananaMan" },
            { "CourierTraceLog.SpiritMachine.txt", "SpiritMachine" },
            { "UmbracoTraceLog.BeepleBrox.txt.2017-07-25", "BeepleBrox" },
            { "UmbracoTraceLog.RD00155D671B04.txt", "RD00155D671B04" },
        };

        /// <summary>
        /// Checks the parsing of machine names from log file names
        /// </summary>
        [TestMethod]
        public void Should_Extract_MachineNames()
        {
            foreach (var entry in logFileNames)
            {
                string machineName = LogFileService.GetMachineName(entry.Key);

                Assert.AreEqual(entry.Value, machineName, "The machine name should be {0} but was {1}", entry.Value, machineName);
            }
        }
    }
}
