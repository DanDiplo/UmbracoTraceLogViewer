using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Diplo.TraceLogViewer.Services;
using NUnit.Framework;

namespace Diplo.TraceLogViewer.Tests
{
    /// <summary>
    /// Umbraco 7.1.x - 7.2.x log format parsing tests
    /// </summary>
    [TestFixture]
    public class Umbraco72xTests
    {
        private readonly string[] EntriesToMatch_72x;

        public Umbraco72xTests()
        {
            EntriesToMatch_72x = new string[]
            {
                @"2014-06-26 20:36:23,372 [10] INFO  Umbraco.Core.PluginManager - [Thread 1] Determining hash of code files on disk",
                @"2015-08-10 20:10:24,363 [30] WARN  umbraco.content - [P7388/T21/D9] Failed to load Xml, file does not exist.",
                @"2015-08-10 20:12:51,344 [52] ERROR Umbraco.Core.UmbracoApplicationBase - [P7388/T50/D12] An unhandled exception occurred
                    System.Configuration.ConfigurationErrorsException: An error occurred creating the configuration section handler for system.data: Column 'InvariantName' is constrained to be unique.  Value 'MySql.Data.MySqlClient' is already present. (D:\Websites\Umbraco\V7 Test\UmbracoCms.7.2.8\web.config line 62) ---> System.Data.ConstraintException: Column 'InvariantName' is constrained to be unique.  Value 'MySql.Data.MySqlClient' is already present.
                        at System.Data.UniqueConstraint.CheckConstraint(DataRow row, DataRowAction action)
                        at System.Data.DataTable.RaiseRowChanging(DataRowChangeEventArgs args, DataRow eRow, DataRowAction eAction, Boolean fireEvent)
                        at System.Data.DataTable.SetNewRecordWorker(DataRow row, Int32 proposedRecord, DataRowAction action, Boolean isInMerge, Boolean suppressEnsurePropertyChanged, Int32 position, Boolean fireEvent, Exception& deferredException)
                        at System.Data.DataTable.InsertRow(DataRow row, Int64 proposedID, Int32 pos, Boolean fireEvent)
                        at System.Data.Common.DbProviderFactoriesConfigurationHandler.DbProviderDictionarySectionHandler.HandleAdd(XmlNode child, DataTable config)
                        at System.Data.Common.DbProviderFactoriesConfigurationHandler.DbProviderDictionarySectionHandler.CreateStatic(DataTable config, Object context, XmlNode section)
                        at System.Data.Common.DbProviderFactoriesConfigurationHandler.CreateStatic(Object parent, Object configContext, XmlNode section)
                        at System.Data.Common.DbProviderFactoriesConfigurationHandler.Create(Object parent, Object configContext, XmlNode section)
                        at System.Configuration.RuntimeConfigurationRecord.RuntimeConfigurationFactory.CreateSectionImpl(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord, Object parentConfig, ConfigXmlReader reader)
                        at System.Configuration.RuntimeConfigurationRecord.RuntimeConfigurationFactory.CreateSectionWithRestrictedPermissions(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord, Object parentConfig, ConfigXmlReader reader)
                        at System.Configuration.RuntimeConfigurationRecord.CreateSection(Boolean inputIsTrusted, FactoryRecord factoryRecord, SectionRecord sectionRecord, Object parentConfig, ConfigXmlReader reader)
                        at System.Configuration.BaseConfigurationRecord.CallCreateSection(Boolean inputIsTrusted, FactoryRecord factoryRecord, SectionRecord sectionRecord, Object parentConfig, ConfigXmlReader reader, String filename, Int32 line)
                        --- End of inner exception stack trace ---
                        at System.Configuration.BaseConfigurationRecord.EvaluateOne(String[] keys, SectionInput input, Boolean isTrusted, FactoryRecord factoryRecord, SectionRecord sectionRecord, Object parentResult)
                        at System.Configuration.BaseConfigurationRecord.Evaluate(FactoryRecord factoryRecord, SectionRecord sectionRecord, Object parentResult, Boolean getLkg, Boolean getRuntimeObject, Object& result, Object& resultRuntimeObject)
                        at System.Configuration.BaseConfigurationRecord.GetSectionRecursive(String configKey, Boolean getLkg, Boolean checkPermission, Boolean getRuntimeObject, Boolean requestIsHere, Object& result, Object& resultRuntimeObject)
                        at System.Configuration.BaseConfigurationRecord.GetSection(String configKey)
                        at System.Web.HttpContext.GetSection(String sectionName)
                        at System.Web.Configuration.HttpConfigurationSystem.GetSection(String sectionName)
                        at System.Web.Configuration.HttpConfigurationSystem.System.Configuration.Internal.IInternalConfigSystem.GetSection(String configKey)
                        at System.Configuration.ConfigurationManager.GetSection(String sectionName)
                        at System.Configuration.PrivilegedConfigurationManager.GetSection(String sectionName)
                        at System.Data.Common.DbProviderFactories.Initialize()
                        at System.Data.Common.DbProviderFactories.GetFactory(String providerInvariantName)
                        at Umbraco.Core.Persistence.Database.CommonConstruct()
                        at Umbraco.Core.Persistence.Database..ctor(String connectionStringName)
                        at Umbraco.Core.Persistence.DefaultDatabaseFactory.CreateDatabase()
                        at Umbraco.Core.DatabaseContext.get_DatabaseProvider()
                        at Umbraco.Core.DatabaseContext.get_CanConnect()
                        at Umbraco.Core.CoreBootManager.EnsureDatabaseConnection()
                        at Umbraco.Core.CoreBootManager.Complete(Action`1 afterComplete)
                        at Umbraco.Web.WebBootManager.Complete(Action`1 afterComplete)
                        at Umbraco.Core.UmbracoApplicationBase.StartApplication(Object sender, EventArgs e)
                        at Umbraco.Core.UmbracoApplicationBase.Application_Start(Object sender, EventArgs e)",
                @"2015-08-10 20:17:01,788 [10] INFO  umbraco.BusinessLogic.Log - [P7388/T21/D16] Log scrubbed.  Removed all items older than 2015-06-11 20:17:01",
                @"2015-08-10 20:10:09,043 [11] ERROR UmbracoExamine.DataServices.UmbracoLogService - [P7389/T1/D2] Provider=InternalMemberIndexer, NodeId=-1
                    System.Exception: Cannot index queue items, the index doesn't exist!,, IndexSet: InternalMemberIndexSet",
                @"2014-06-26 20:36:47,340 [10] INFO  Umbraco.Core.ApplicationContext - [Thread 11] CurrentVersion different from configStatus: '7.1.2',''-]",
                @"2016-01-20 11:11:28,402 [10] WARN Server: NOT AVAILABLE Url: http://NOT AVAILABLENOT AVAILABLE Umbraco.Core.Services.ApplicationTreeService - [P12108/T19/D2] The tree definition: <add initialize=""true"" sortOrder=""0"" alias=""censusTree"" application=""developer"" title=""Census"" iconClosed=""icon-folder"" iconOpen=""icon-folder-open"" type=""Census.Controllers.CensusTreeController, Census"" /> could not be resolved to a .Net object type",
                @"2015-07-22 20:17:16,194 [8] INFO Umbraco.Core.CoreBootManager - [Thread 14] Umbraco 7.2.8 [banana] application starting on DONKEY2000",
                @"2014-05-22 21:12:15,506 [17] INFO   Umbraco.Core.Persistence.Database - [Thread 13] Create Table sql -1:
                     CREATE TABLE [umbracoUserLogins] ([contextID] UniqueIdentifier NOT NULL,
                    [userID] INTEGER NOT NULL,
                    [timeout] BIGINT NOT NULL)"
            };
        }

        [Test]
        public void Should_Read_Umbraco72x_LogFile()
        {
            var logFile = Path.Combine(Configuration.TestLogsDirectory, Configuration.Umbraco72xFile);
            TestContext.Write(logFile);

            LogDataService dataService = new LogDataService();

            var logData = dataService.GetLogDataFromFilePath(logFile);

            int logDataEntryCount = logData.Count();

            Assert.That(logDataEntryCount, Is.EqualTo(29), "The log data entry count should be 29 but is actually {0}", logDataEntryCount);
        }

        [Test]
        public void Should_Match_All_Umbraco_72x_Entries()
        {
            foreach (var entry in EntriesToMatch_72x)
            {
                var match = LogDataService.CheckIsLongEntryMatch(entry);
                Assert.That(match.Success, "The entry '{0}' was not considered a match", entry);
            }
        }

        [Test]
        public void Check_Entry_One_72x_Is_Parsed()
        {
            var entry = EntriesToMatch_72x[0];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2014-06-26 20:36:23");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("INFO"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Core.PluginManager"));

            Assert.That(data.Message, Is.EqualTo("Determining hash of code files on disk"));

            Assert.That(data.ThreadId, Is.EqualTo("1"));
        }

        [Test]
        public void Check_Entry_Two_72x_Is_Parsed()
        {
            var entry = EntriesToMatch_72x[1];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2015-08-10 20:10:24");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("WARN"));

            Assert.That(data.Logger, Is.EqualTo("umbraco.content"));

            Assert.That(data.Message, Is.EqualTo("Failed to load Xml, file does not exist."));

            Assert.That(data.ThreadId, Is.EqualTo("21"));

            Assert.That(data.ProcessId, Is.EqualTo("7388"));

            Assert.That(data.DomainId, Is.EqualTo("9"));
        }

        [Test]
        public void Check_Entry_Three_72x_Is_Parsed()
        {
            var entry = EntriesToMatch_72x[2];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2015-08-10 20:12:51");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("ERROR"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Core.UmbracoApplicationBase"));

            Assert.That(data.Message, Does.StartWith("An unhandled exception occurred"));

            Assert.That(data.Message, Does.EndWith("at Umbraco.Core.UmbracoApplicationBase.Application_Start(Object sender, EventArgs e)"));

            Assert.That(data.ThreadId, Is.EqualTo("50"));

            Assert.That(data.ProcessId, Is.EqualTo("7388"));

            Assert.That(data.DomainId, Is.EqualTo("12"));
        }

        [Test]
        public void Check_Entry_Four_72x_Is_Parsed()
        {
            var entry = EntriesToMatch_72x[3];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2015-08-10 20:17:01");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("INFO"));

            Assert.That(data.Logger, Is.EqualTo("umbraco.BusinessLogic.Log"));

            Assert.That(data.Message, Is.EqualTo("Log scrubbed.  Removed all items older than 2015-06-11 20:17:01"));

            Assert.That(data.ThreadId, Is.EqualTo("21"));

            Assert.That(data.ProcessId, Is.EqualTo("7388"));

            Assert.That(data.DomainId, Is.EqualTo("16"));
        }

        [Test]
        public void Check_Entry_Five_72x_Is_Parsed()
        {
            var entry = EntriesToMatch_72x[4];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2015-08-10 20:10:09");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("ERROR"));

            Assert.That(data.Logger, Is.EqualTo("UmbracoExamine.DataServices.UmbracoLogService"));

            Assert.That(data.Message, Does.StartWith("Provider=InternalMemberIndexer, NodeId=-1"));

            Assert.That(data.Message, Does.EndWith("System.Exception: Cannot index queue items, the index doesn't exist!,, IndexSet: InternalMemberIndexSet"));

            Assert.That(data.ThreadId, Is.EqualTo("1"));

            Assert.That(data.ProcessId, Is.EqualTo("7389"));

            Assert.That(data.DomainId, Is.EqualTo("2"));
        }

        [Test]
        public void Check_Entry_Six_72x_Is_Parsed()
        {
            var entry = EntriesToMatch_72x[5];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2014-06-26 20:36:47");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("INFO"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Core.ApplicationContext"));

            Assert.That(data.Message, Does.StartWith("CurrentVersion different from configStatus: '7.1.2',''-]"));

            Assert.That(data.ThreadId, Is.EqualTo("11"));
        }

        [Test]
        public void Check_Entry_Seven_72x_Is_Parsed()
        {
            var entry = EntriesToMatch_72x[6];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2016-01-20 11:11:28");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("WARN"));

            Assert.That(data.Logger, Is.EqualTo("Server: NOT AVAILABLE Url: http://NOT AVAILABLENOT AVAILABLE Umbraco.Core.Services.ApplicationTreeService"));

            Assert.That(data.Message, Does.StartWith(@"The tree definition: <add initialize=""true"" sortOrder=""0"" alias=""censusTree"" application=""developer"" title=""Census"" iconClosed=""icon-folder"" iconOpen=""icon-folder-open"" type=""Census.Controllers.CensusTreeController, Census"" /> could not be resolved to a .Net object type"));

            Assert.That(data.ThreadId, Is.EqualTo("19"));

            Assert.That(data.ProcessId, Is.EqualTo("12108"));

            Assert.That(data.DomainId, Is.EqualTo("2"));
        }

        [Test]
        public void Check_Entry_Eight_72x_Is_Parsed()
        {
            var entry = EntriesToMatch_72x[7];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2015-07-22 20:17:16");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("INFO"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Core.CoreBootManager"));

            Assert.That(data.Message, Is.EqualTo("Umbraco 7.2.8 [banana] application starting on DONKEY2000"));

            Assert.That(data.ThreadId, Is.EqualTo("14"));
        }

        [Test]
        public void Check_Entry_Nine_72x_Is_Parsed()
        {
            var entry = EntriesToMatch_72x[8];

            var match = LogDataService.CheckIsLongEntryMatch(entry);
            var data = LogDataService.ParseLogDataItem(entry, match);

            Assert.That(data, Is.Not.Null);

            var expectedDate = DateTime.Parse("2014-05-22 21:12:15");
            Assert.That(data.Date, Is.EqualTo(expectedDate));

            Assert.That(data.Level, Is.EqualTo("INFO"));

            Assert.That(data.Logger, Is.EqualTo("Umbraco.Core.Persistence.Database"));

            Assert.That(data.Message, Does.StartWith("Create Table sql -1:"));

            Assert.That(data.Message, Does.EndWith("[timeout] BIGINT NOT NULL)"));

            Assert.That(data.ThreadId, Is.EqualTo("13"));
        }
    }
}
