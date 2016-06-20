using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Workflows.Simple;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarstan.Workflows
{
    public class BundleAction
    {
        /// <summary>
        /// Runs the processor.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Process(WorkflowPipelineArgs args)
        {
            var commandName = args.CommandItem.Name;
            var comments = string.Format("Bundled with {0} ({1} - {2})", args.DataItem.Name, args.DataItem.Paths.FullPath, args.DataItem.ID);

            //Find all related
            var relatedItems = GetRenderingDataSourceItems(args.DataItem);
            foreach (var relatedItem in relatedItems)
            {
                if (relatedItem != null)
                {
                    //verify workflow is module workflow
                    if (relatedItem != null && relatedItem.Fields[FieldIDs.Workflow] != null
                        && !string.IsNullOrEmpty(relatedItem.Fields[FieldIDs.WorkflowState].Value))
                    {
                        try
                        {
                            var workflowProvider = relatedItem.Database.WorkflowProvider;
                            if (workflowProvider != null)
                            {
                                var workflow = workflowProvider.GetWorkflow(relatedItem);
                                //if no command is found, then the workflow isn't in the same state, it's not set, or it's a different workflow
                                var command = workflow.GetCommands(relatedItem).Select(c => relatedItem.Database.SelectSingleItem(c.CommandID)).FirstOrDefault(i => i.Name == commandName);
                                if (command != null && workflow != null)
                                {
                                    var result = workflow.Execute(command.ID.ToString(), relatedItem, comments, false);

                                    if (!result.Succeeded)
                                    {
                                        Log.Info(string.Format("INFO: Bundling item ({0} - {1}) failed: {2}", relatedItem.ID, relatedItem.Paths.FullPath, result.Message), typeof(BundleAction));
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(string.Format("Error bundling item ({0} - {1})", relatedItem.ID, relatedItem.Paths.FullPath), ex, typeof(BundleAction));
                        }
                    }
                }
            }

            Log.Info(string.Format("INFO: Bundled {0} items with Bundled with {1} ({2} - {3})", relatedItems.Count, args.DataItem.Name, args.DataItem.Paths.FullPath, args.DataItem.ID), typeof(BundleAction));
        }

        private List<Item> GetRenderingDataSourceItems(Item item)
        {
            var items = new List<Item>();
            var renderings = item.Visualization.GetRenderings(Sitecore.Context.Device, true);

            foreach (var rendering in renderings)
            {
                //This check ensures only items are added, not queries
                if (Sitecore.Data.ID.IsID(rendering.Settings.DataSource))
                {
                    var dsItem = item.Database.SelectSingleItem(rendering.Settings.DataSource);
                    if (dsItem != null)
                    {
                        //Add the datasource item
                        items.Add(dsItem);
                    }
                }
            }
            return items;
        }
    }
}
