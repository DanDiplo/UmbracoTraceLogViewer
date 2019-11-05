using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using Diplo.TraceLogViewer.Models;
using Diplo.TraceLogViewer.Services;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi.Filters;

namespace Diplo.TraceLogViewer.Controllers
{
    /// <summary>
    /// Diplo TraceLog Tree Controller
    /// </summary>
    /// <remarks>
    /// Creates the tree for the tracelogs, with log files going in Date and Filename folders
    /// </remarks>
    [UmbracoApplicationAuthorize(Constants.Applications.Developer)]
    [Tree(Constants.Applications.Developer, "diploTraceLog", "Trace Logs", sortOrder: 9)]
    [PluginController("DiploTraceLogViewer")]
    public class TraceLogTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection qs)
        {
            var tree = new TreeNodeCollection();
            var service = new LogFileService();
            var logFiles = service.GetLogFiles();

            if (logFiles == null || !logFiles.Any())
            {
                return tree;
            }

            var recent = logFiles.Where(l => l.Date.Date == DateTime.Today);

            if (!recent.Any())
            {
                recent = logFiles.FirstOrDefault().AsEnumerableOfOne();
            }

            if (id == Constants.System.Root.ToInvariantString())
            {
                if (recent.Any())
                {
                    foreach (var recentLogFile in recent)
                    {
                        CreateDateLogTreeItem(id, qs, tree, recentLogFile);
                    }
                }

                tree.Add(CreateTreeNode("Date", id, qs, "Dates", "icon-folder", true));
                tree.Add(CreateTreeNode("Filename", id, qs, "Filenames", "icon-folder", true));
            }

            if (id == "Date")
            {
                this.AddDateRangeTree(tree, id, qs, logFiles);
            }

            if (id == "Filename")
            {
                this.AddFileNameTree(tree, id, qs, logFiles);
            }

            return tree;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                menu.Items.Add<RefreshNode, ActionRefresh>("Reload Log Files", true);
            }

            return menu;
        }

        /// <summary>
        /// Adds the tree that shows log dates in the Date folder
        /// </summary>
        /// <param name="tree">The tree</param>
        /// <param name="id">The parent tree Id</param>
        /// <param name="qs">The query string</param>
        /// <param name="logFiles">The log files collection</param>
        private void AddDateRangeTree(TreeNodeCollection tree, string id, FormDataCollection qs, IEnumerable<LogFileItem> logFiles)
        {
            foreach (var logFile in logFiles)
            {
                CreateDateLogTreeItem(id, qs, tree, logFile);
            }
        }

        /// <summary>
        /// Adds the tree that shows the log file names in the Filename folder
        /// </summary>
        /// <param name="tree">The tree</param>
        /// <param name="id">The parent tree Id</param>
        /// <param name="qs">The query string</param>
        /// <param name="logFiles">The log files collection</param>
        private void AddFileNameTree(TreeNodeCollection tree, string id, FormDataCollection qs, IEnumerable<LogFileItem> logFiles)
        {
            foreach (var logFile in logFiles)
            {
                string title = System.IO.Path.GetFileName(logFile.Path);
                string path = HttpUtility.UrlEncode(title);
                tree.Add(CreateTreeNode(path, id, qs, title, "icon-notepad"));
            }
        }

        private void CreateDateLogTreeItem(string id, FormDataCollection qs, TreeNodeCollection tree, LogFileItem logFile)
        {
            string title = logFile.Date.ToString("yyyy-MM-dd");

            if (!string.IsNullOrEmpty(logFile.MachineName) && !logFile.MachineName.InvariantEquals(Environment.MachineName))
            {
                title += string.Format(" ({0})", logFile.MachineName);
            }

            if (logFile.IsCourier)
            {
                title += " (C)";
            }

            string path = HttpUtility.UrlEncode(System.IO.Path.GetFileName(logFile.Path));
            tree.Add(CreateTreeNode(path, id, qs, title, "icon-notepad"));
        }
    }
}
