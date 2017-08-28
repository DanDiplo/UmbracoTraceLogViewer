using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Diplo.TraceLogViewer.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Diplo.TraceLogViewer.Tests
{
    [TestClass]
    public class LogFileTests
    {
        private readonly string[] EntriesToMatch;

        public LogFileTests()
        {
            EntriesToMatch = new string[]
           {
                @"2016-01-21 22:14:10,559 [P10176/D2/T1] INFO  Umbraco.Core.CoreBootManager - Umbraco 7.3.0 application starting on COMPUTER",
                @"2016-01-21 22:16:42,289 [P10176/D4/T30] WARN  Umbraco.Web.UmbracoModule - Status code is 404 yet TrySkipIisCustomErrors is false - IIS will take over.",
                @"2016-01-21 22:16:47,114 [P10176/D4/T7] ERROR Umbraco.Web.WebApi.Filters.AngularAntiForgeryHelper - Could not validate XSRF token
                System.Web.Mvc.HttpAntiForgeryException (0x80004005): The provided anti-forgery token was meant for user ""dan@diplo.co.uk"", but the current user is ""Admin"".
                   at System.Web.Helpers.AntiXsrf.TokenValidator.ValidateTokens(HttpContextBase httpContext, IIdentity identity, AntiForgeryToken sessionToken, AntiForgeryToken fieldToken)
                   at System.Web.Helpers.AntiXsrf.AntiForgeryWorker.Validate(HttpContextBase httpContext, String cookieToken, String formToken)
                   at System.Web.Helpers.AntiForgery.Validate(String cookieToken, String formToken)
                   at Umbraco.Web.WebApi.Filters.AngularAntiForgeryHelper.ValidateTokens(String cookieToken, String headerToken)",
                @"2015-09-16 16:41:08,651 [P7548/T42/D8] ERROR umbraco.content - Failed to load Xml from file.
                    System.Xml.XmlException: There is no Unicode byte order mark. Cannot switch to Unicode.
                       at System.Xml.XmlTextReaderImpl.Throw(Exception e)
                       at System.Xml.XmlTextReaderImpl.ThrowWithoutLineInfo(String res)
                       at System.Xml.XmlTextReaderImpl.CheckEncoding(String newEncodingName)
                       at System.Xml.XmlTextReaderImpl.ParseXmlDeclaration(Boolean isTextDecl)
                       at System.Xml.XmlTextReaderImpl.Read()
                       at System.Xml.XmlLoader.Load(XmlDocument doc, XmlReader reader, Boolean preserveWhitespace)
                       at System.Xml.XmlDocument.Load(XmlReader reader)
                       at System.Xml.XmlDocument.Load(Stream inStream)
                       at umbraco.content.LoadXmlFromFile()",
                @"2015-11-17 09:03:59,502 [P5308/T1/D2] ERROR Umbraco.Core.UmbracoApplicationBase - An unhandled exception occurred 
                    System.Web.HttpCompileException (0x80004005): ext controllerContext, String[] locations, String[]
                    And lots of other messages here some with square brackets in like [these]...",
                @"2015-12-14 10:26:51,147 [P8060/D2/T9] INFO  Umbraco.Core.UmbracoApplicationBase - Application shutdown. Details: ConfigurationChange

                    _shutDownMessage=CONFIG change
                    HostingEnvironment initiated shutdown
                    CONFIG change
                    Change Notification for critical directories.
                    bin dir change or directory rename
                    Change in C:\Users\Dan\AppData\Local\Temp\Temporary ASP.NET Files\vs\3f092064\cdfa142a\hash\hash.web
                    Change in C:\Users\Dan\AppData\Local\Temp\Temporary ASP.NET Files\vs\3f092064\cdfa142a\hash\hash.web
                    CONFIG change
                    CONFIG change
                    CONFIG change
                    CONFIG change
                    CONFIG change
                    CONFIG change
                    HostingEnvironment caused shutdown

                    _shutDownStack=   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
                       at System.Environment.get_StackTrace()
                       at System.Web.Hosting.HostingEnvironment.InitiateShutdownInternal()
                       at System.Web.Hosting.HostingEnvironment.InitiateShutdownWithoutDemand()
                       at System.Web.HttpRuntime.ShutdownAppDomain(String stackTrace)
                       at System.Web.Configuration.HttpConfigurationSystem.OnConfigurationChanged(Object sender, InternalConfigEventArgs e)
                       at System.Configuration.Internal.InternalConfigRoot.OnConfigChanged(InternalConfigEventArgs e)
                       at System.Configuration.BaseConfigurationRecord.OnStreamChanged(String streamname)
                       at System.Web.Configuration.WebConfigurationHostFileChange.OnFileChanged(Object sender, FileChangeEvent e)
                       at System.Web.DirectoryMonitor.FireNotifications()
                       at System.Web.Util.WorkItem.CallCallbackWithAssert(WorkItemCallback callback)
                       at System.Web.Util.WorkItem.OnQueueUserWorkItemCompletion(Object state)
                       at System.Threading.QueueUserWorkItemCallback.WaitCallback_Context(Object state)
                       at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
                       at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
                       at System.Threading.QueueUserWorkItemCallback.System.Threading.IThreadPoolWorkItem.ExecuteWorkItem()
                       at System.Threading.ThreadPoolWorkQueue.Dispatch()
                       at System.Threading._ThreadPoolWaitCallback.PerformWaitCallback()",
                @"2015-08-26 17:08:16,209 [P3732/D16/T33] INFO  Umbraco.Web.Scheduling.BackgroundTaskRunner - [KeepAlive] Terminating"
           };
        }


        [TestMethod]
        public void Should_Read_Umbraco_LogFile()
        {
            var logFile = Path.Combine(Configuration.TestLogsDirectory, Configuration.UmbracoFile);

            LogDataService dataService = new LogDataService();

            var logData = dataService.GetLogDataFromFilePath(logFile);

            int logDataEntryCount = logData.Count();

            Assert.AreEqual(24, logDataEntryCount, "The log data entry count should be 24 but is actually {0}", logDataEntryCount);
        }

        [TestMethod]
        public void Should_Match_All_Log_Entries()
        {
            foreach (var entry in EntriesToMatch)
            {
                var match = LogDataService.CheckIsLongEntryMatch(entry);
                Assert.AreEqual(true, match.Success, "The entry '{0}' was not considered a match", entry);
            }
        }

        [TestMethod]
        public void Check_Entry_One_Is_Parsed()
        {
            var entry = EntriesToMatch[0];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.IsNotNull(data);

            var expectedDate = DateTime.Parse("2016-01-21 22:14:10.559");
            Assert.AreEqual(expectedDate, data.Date);

            Assert.AreEqual("INFO", data.Level);

            Assert.AreEqual("Umbraco.Core.CoreBootManager", data.Logger);

            Assert.AreEqual("Umbraco 7.3.0 application starting on COMPUTER", data.Message.Substring(0, 46));

            Assert.AreEqual("1", data.ThreadId);

            Assert.AreEqual("10176", data.ProcessId);

            Assert.AreEqual("2", data.DomainId);
        }

        [TestMethod]
        public void Check_Entry_Two_Is_Parsed()
        { 
            var entry = EntriesToMatch[1];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.IsNotNull(data);

            var expectedDate = DateTime.Parse("2016-01-21 22:16:42.289");
            Assert.AreEqual(expectedDate, data.Date);

            Assert.AreEqual("WARN", data.Level);

            Assert.AreEqual("Umbraco.Web.UmbracoModule", data.Logger);

            Assert.AreEqual("Status code is 404 yet TrySkipIisCustomErrors is false - IIS will take over.", data.Message);

            Assert.AreEqual("30", data.ThreadId);

            Assert.AreEqual("10176", data.ProcessId);

            Assert.AreEqual("4", data.DomainId);
        }

        [TestMethod]
        public void Check_Entry_Three_Is_Parsed()
        {
            var entry = EntriesToMatch[2];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.IsNotNull(data);

            var expectedDate = DateTime.Parse("2016-01-21 22:16:47.114");
            Assert.AreEqual(expectedDate, data.Date);

            Assert.AreEqual("ERROR", data.Level);

            Assert.AreEqual("Umbraco.Web.WebApi.Filters.AngularAntiForgeryHelper", data.Logger);

            Assert.IsTrue(data.Message.Contains("at Umbraco.Web.WebApi.Filters.AngularAntiForgeryHelper.ValidateTokens(String cookieToken, String headerToken)"));

            Assert.AreEqual("7", data.ThreadId);

            Assert.AreEqual("10176", data.ProcessId);

            Assert.AreEqual("4", data.DomainId);
        }

        [TestMethod]
        public void Check_Entry_Four_Is_Parsed()
        {
            var entry = EntriesToMatch[3];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.IsNotNull(data);

            var expectedDate = DateTime.Parse("2015-09-16 16:41:08.651");
            Assert.AreEqual(expectedDate, data.Date);

            Assert.AreEqual("ERROR", data.Level);

            Assert.AreEqual("umbraco.content", data.Logger);

            Trace.Write(data.Message);

            Assert.IsTrue(data.Message.StartsWith("Failed to load Xml from file."));

            Assert.AreEqual("42", data.ThreadId);

            Assert.AreEqual("7548", data.ProcessId);

            Assert.AreEqual("8", data.DomainId);
        }


        [TestMethod]
        public void Check_Entry_Five_Is_Parsed()
        {
            var entry = EntriesToMatch[4];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.IsNotNull(data);

            var expectedDate = DateTime.Parse("2015-11-17 09:03:59.502");
            Assert.AreEqual(expectedDate, data.Date);

            Assert.AreEqual("ERROR", data.Level);

            Assert.AreEqual("Umbraco.Core.UmbracoApplicationBase", data.Logger);

            Trace.Write(data.Message);

            Assert.IsTrue(data.Message.StartsWith("An unhandled exception occurred"));

            Assert.IsTrue(data.Message.EndsWith("And lots of other messages here some with square brackets in like [these]..."));

            Assert.AreEqual("1", data.ThreadId);

            Assert.AreEqual("5308", data.ProcessId);

            Assert.AreEqual("2", data.DomainId);
        }

        [TestMethod]
        public void Check_Entry_Six_Is_Parsed()
        {
            var entry = EntriesToMatch[5];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);


            Assert.IsNotNull(data);

            var expectedDate = DateTime.Parse("2015-12-14 10:26:51.147");
            Assert.AreEqual(expectedDate, data.Date);

            Assert.AreEqual("INFO", data.Level);

            Assert.AreEqual("Umbraco.Core.UmbracoApplicationBase", data.Logger);

            Trace.Write(data.Message);

            Assert.IsTrue(data.Message.StartsWith("Application shutdown. Details: ConfigurationChange"));

            Assert.IsTrue(data.Message.EndsWith("at System.Threading._ThreadPoolWaitCallback.PerformWaitCallback()"));

            Assert.AreEqual("9", data.ThreadId);

            Assert.AreEqual("8060", data.ProcessId);

            Assert.AreEqual("2", data.DomainId);
        }

        [TestMethod]
        public void Check_Entry_Seven_Is_Parsed()
        {
            var entry = EntriesToMatch[6];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.IsNotNull(data);

            var expectedDate = DateTime.Parse("2015-08-26 17:08:16.209");
            Assert.AreEqual(expectedDate, data.Date);

            Assert.AreEqual("INFO", data.Level);

            Assert.AreEqual("Umbraco.Web.Scheduling.BackgroundTaskRunner", data.Logger);

            Assert.AreEqual("[KeepAlive] Terminating", data.Message);

            Assert.AreEqual("33", data.ThreadId);

            Assert.AreEqual("3732", data.ProcessId);

            Assert.AreEqual("16", data.DomainId);
        }
    }
}
