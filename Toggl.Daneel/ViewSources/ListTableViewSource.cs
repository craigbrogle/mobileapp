using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public class ListTableViewSource<TModel, TCell> : UITableViewSource where TCell : UITableViewCell
    {
        private readonly IList<TModel> items;
        private readonly string cellIdentifier;
        private readonly Func<UITableViewCell, TModel, TCell> configureCell;

        public ListTableViewSource(IEnumerable<TModel> items, string cellIdentifier, Func<UITableViewCell, TModel, TCell> configureCell)
        {
            this.items = items.ToList();
            this.cellIdentifier = cellIdentifier;
            this.configureCell = configureCell;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier, indexPath);
            var model = items[indexPath.Row];
            return configureCell(cell, model);
        }

        public override nint RowsInSection(UITableView tableview, nint section) => items.Count;
    }
}
