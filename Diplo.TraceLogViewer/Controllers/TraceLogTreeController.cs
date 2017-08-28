using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
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
	/// Creates the tree for the tracelogs, with each logfile as a separate node
	/// </remarks>
    [UmbracoApplicationAuthorize(Constants.Applications.Developer)]
	[Tree(Constants.Applications.Developer, "diploTraceLog", "Trace Logs", sortOrder:9)]
	[PluginController("DiploTraceLogViewer")]
	public class TraceLogTreeController : TreeController
	{
		protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection qs)
		{
            TreeNodeCollection tree = new TreeNodeCollection();
            LogFileService service = new LogFileService();
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
                    foreach (var logFile in recent)
                    {
                        CreateDateLogTreeItem(id, qs, tree, logFile);
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

        private void AddDateRangeTree(TreeNodeCollection tree, string id, FormDataCollection qs, IEnumerable<LogFileItem> logFiles)
        {
            foreach (var logFile in logFiles)
            {
                CreateDateLogTreeItem(id, qs, tree, logFile);
            }
        }

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
            string path = HttpUtility.UrlEncode(System.IO.Path.GetFileName(logFile.Path));
            tree.Add(CreateTreeNode(path, id, qs, title, "icon-notepad"));
        }

        private TreeNodeCollection PopulateTreeNodes(string parentId, FormDataCollection qs)
		{
			TreeNodeCollection tree = new TreeNodeCollection();
			LogFileService service = new LogFileService();
            string currentMachineName = Environment.MachineName;

            foreach (var logFile in service.GetLogFiles())
			{
                string title = logFile.Date.ToString("yyyy-MM-dd");

                if (logFile.MachineName != null && !logFile.MachineName.InvariantEquals(currentMachineName))
                {
                    title += " (" + logFile.MachineName + ")";
                }

				string path = HttpUtility.UrlEncode(System.IO.Path.GetFileName(logFile.Path));

				tree.Add(CreateTreeNode(path, parentId, qs, title, "icon-notepad"));
			}

			return tree;
		}
	}
}
