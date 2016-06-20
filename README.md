# Sitecore Workflow Bundling

If you are using the Datasource field on renderings that are being added to a page, then Sitecore Workflow Bundling can help you with managing workflow on these items.

Example:
A page item is in the "Draft" state, but has several renderings, each with Datasources pointed to Sitecore items that also have Workflow assigned, also in a "Draft" state.  When the page item is Submitted, and moved to the "Awaiting Approval" workflow state, the items that are being referenced by the Datasource field of the page's renderings will also be moved to the "Awaiting Approval" state.
