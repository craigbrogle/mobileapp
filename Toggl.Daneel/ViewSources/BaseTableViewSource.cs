using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Toggl.Daneel.Cells;

namespace Toggl.Daneel.ViewSources
{
    public abstract class BaseTableViewSource<T> : UITableViewSource
    {
        protected UITableView TableView { get; }

        public Func<T, Task> OnItemTapped { get; set; }

        private IList<T> items = new List<T>();
        public virtual IList<T> Items
        {
            get => items;
            set => SetItems(value ?? new List<T>());
        }

        protected BaseTableViewSource(UITableView tableView)
        {
            TableView = tableView;
        }

        protected BaseTableViewSource(IntPtr handle)
            : base(handle)
        {
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItem(indexPath);
            var cell = GetOrCreateCellFor(tableView, indexPath, item);
            cell.Item = item;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
            => Items.Count;

        protected abstract BaseTableCell<T> GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item);

        public virtual T GetItem(NSIndexPath viewPosition)
            => items[viewPosition.Row];

        protected virtual void SetItems(IList<T> items)
        {
            this.items = items;

            TableView.ReloadData();
        }
    }
}
