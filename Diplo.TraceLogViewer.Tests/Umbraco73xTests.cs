using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Diplo.TraceLogViewer.Services;
using NUnit.Framework;

namespace Diplo.TraceLogViewer.Tests
{
    /// <summary>
    /// Umbraco 7.3.x log format parsing tests
    /// </summary>
    [TestFixture]
    public class Umbraco73xTests
    {
        private readonly string[] EntriesToMatch_73x;

        public Umbraco73xTests()
        {
            EntriesToMatch_73x = new string[]
            {
                @"2016-01-21 22:14:10,559 [P10176/D2/T1] INFO  Umbraco.Core.CoreBootManager - Umbraco 7.3.0 application starting on COMPUTER",
                @"2016-01-21 22:16:42,289 [P10176/D4/T30] WARN  Umbraco.Web.UmbracoModule - Status code is 404 yet TrySkipIisCustomErrors is false - IIS will take over.",
                @"2016-01-21 22:16:47,114 [P10176/D4/T7] ERROR Umbraco.Web.WebApi.Filters.AngularAntiForgeryHelper - Could not validate XSRF token
                System.Web.Mvc.HttpAntiForgeryException (0x80004005): The provided anti-forgery token was meant for user ""dan@diplo.co.uk"", but the current user is ""Admin"".
                   at System.Web.Helpers.AntiXsrf.TokenValidator.ValidateTokens(HttpContextBase httpContext, IIdentity identity, AntiForgeryToken sessionToken, AntiForgeryToken fieldToken)
                   at System.Web.Helpers.AntiXsrf.AntiForgeryWorker.Validate(HttpContextBase httpContext, String cookieToken, String formToken)
                   at System.Web.Helpers.AntiForgery.Validate(String cookieToken, String formToken)
                   at Umbraco.Web.WebApi.Filters.AngularAntiForgeryHelper.ValidateTokens(String cookieToken, String headerToken)",
                @"2015-09-16 16:41:08,651 [36] ERROR umbraco.content - [P7548/T42/D8] Failed to load Xml from file.
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
                @"2015-11-17 09:03:59,502 [9] ERROR Umbraco.Core.UmbracoApplicationBase - [P5308/T1/D2] An unhandled exception occurred 
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

        [Test]
        public void Should_Read_Umbraco73x_LogFile()
        {
            var logFile = Path.Combine(Configuration.TestLogsDirectory, Configuration.Umbraco73xFile);

            LogDataService dataService = new LogDataService();

            var logData = dataService.GetLogDataFromFilePath(logFile);

            int logDataEntryCount = logData.Count();

            Assert.That(logDataEntryCount, Is.EqualTo(24), "The log data entry count should be 24 but is actually {0}", logDataEntryCount);
        }

        [Test]
        public void Should_Match_All_Umbraco_73x_Entries()
        {
            foreach (var entry in EntriesToMatch_73x)
            {
                var match = LogDataService.CheckIsLongEntryMatch(entry);
                Assert.That(match.Success, "The entry '{0}' was not considered a match", entry);
            }
        }

        [Test]
        public void Check_Entry_One_73x_Is_Parsed()
        {
            var entry = EntriesToMatch_73x[0];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2016-01-21 22:14:10.559");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("INFO"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Core.CoreBootManager"));

            Assert.That(data.Message, Does.StartWith("Umbraco 7.3.0 application starting on COMPUTER"));

            Assert.That(data.ThreadId, Is.EqualTo("1"));

            Assert.That(data.ProcessId, Is.EqualTo("10176"));

            Assert.That(data.DomainId, Is.EqualTo("2"));
        }

        [Test]
        public void Check_Entry_Two_73x_Is_Parsed()
        {
            var entry = EntriesToMatch_73x[1];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            TestContext.WriteLine(data);

            var expectedDate = DateTime.Parse("2016-01-21 22:16:42.289");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("WARN"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Web.UmbracoModule"));

            Assert.That(data.Message, Does.StartWith("Status code is 404 yet TrySkipIisCustomErrors is false - IIS will take over."));

            Assert.That(data.ThreadId, Is.EqualTo("30"));

            Assert.That(data.ProcessId, Is.EqualTo("10176"));

            Assert.That(data.DomainId, Is.EqualTo("4"));
        }

        [Test]
        public void Check_Entry_Three_73x_Is_Parsed()
        {
            var entry = EntriesToMatch_73x[2];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2016-01-21 22:16:47.114");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("ERROR"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Web.WebApi.Filters.AngularAntiForgeryHelper"));

            Assert.That(data.Message, Does.StartWith("Could not validate XSRF token"));

            Assert.That(data.Message, Does.EndWith("at Umbraco.Web.WebApi.Filters.AngularAntiForgeryHelper.ValidateTokens(String cookieToken, String headerToken)"));

            Assert.That(data.ThreadId, Is.EqualTo("7"));

            Assert.That(data.ProcessId, Is.EqualTo("10176"));

            Assert.That(data.DomainId, Is.EqualTo("4"));
        }

        [Test]
        public void Check_Entry_Four_73x_Is_Parsed()
        {
            var entry = EntriesToMatch_73x[3];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2015-09-16 16:41:08.651");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("ERROR"));

            Assert.That(data.Logger, Is.EqualTo("umbraco.content"));

            Assert.That(data.Message, Does.StartWith("Failed to load Xml from file."));

            Assert.That(data.Message, Does.EndWith("at umbraco.content.LoadXmlFromFile()"));

            Assert.That(data.ThreadId, Is.EqualTo("42"));

            Assert.That(data.ProcessId, Is.EqualTo("7548"));

            Assert.That(data.DomainId, Is.EqualTo("8"));
        }

        [Test]
        public void Check_Entry_Five_73x_Is_Parsed()
        {
            var entry = EntriesToMatch_73x[4];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2015-11-17 09:03:59.502");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("ERROR"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Core.UmbracoApplicationBase"));

            Assert.That(data.Message, Does.StartWith("An unhandled exception occurred"));

            Assert.That(data.Message, Does.EndWith("And lots of other messages here some with square brackets in like [these]..."));

            Assert.That(data.ThreadId, Is.EqualTo("1"));

            Assert.That(data.ProcessId, Is.EqualTo("5308"));

            Assert.That(data.DomainId, Is.EqualTo("2"));
        }

        [Test]
        public void Check_Entry_Six_73x_Is_Parsed()
        {
            var entry = EntriesToMatch_73x[5];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2015-12-14 10:26:51.147");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("INFO"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Core.UmbracoApplicationBase"));

            Assert.That(data.Message, Does.StartWith("Application shutdown. Details: ConfigurationChange"));

            Assert.That(data.Message, Does.EndWith("at System.Threading._ThreadPoolWaitCallback.PerformWaitCallback()"));

            Assert.That(data.ThreadId, Is.EqualTo("9"));

            Assert.That(data.ProcessId, Is.EqualTo("8060"));

            Assert.That(data.DomainId, Is.EqualTo("2"));
        }

        [Test]
        public void Check_Entry_Seven_73x_Is_Parsed()
        {
            var entry = EntriesToMatch_73x[6];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2015-08-26 17:08:16.209");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("INFO"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Web.Scheduling.BackgroundTaskRunner"));

            Assert.That(data.Message, Is.EqualTo("[KeepAlive] Terminating"));

            Assert.That(data.ThreadId, Is.EqualTo("33"));

            Assert.That(data.ProcessId, Is.EqualTo("3732"));

            Assert.That(data.DomainId, Is.EqualTo("16"));
        }

        [Test]
        public void Time_ParseLargeLogFile()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var logFile = Path.Combine(Configuration.TestLogsDirectory, "UmbracoTraceLog.MictPHC124-PC.txt");

            LogDataService dataService = new LogDataService();

            var logData = dataService.GetLogDataFromFilePath(logFile).Count();

            sw.Stop();

            TestContext.WriteLine("Time taken: " + sw.Elapsed);
        }
    }
}
