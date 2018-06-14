using System;
using Foundation;
using Toggl.Daneel.Cells;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public class SimpleTableViewSource<TItem, TCell> : BaseTableViewSource<TItem>
        where TCell : BaseTableCell<TItem>
    {
        protected SimpleTableViewSource(UITableView tableView)
            : base(tableView)
        {
            TableView.RegisterClassForCellReuse(typeof(TCell), nameof(TCell));
        }

        protected SimpleTableViewSource(IntPtr handle)
            : base(handle)
        {
        }

        protected override BaseTableCell<TItem> GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => TableView.DequeueReusableCell(nameof(TCell)) as BaseTableCell<TItem>;
    }
}
