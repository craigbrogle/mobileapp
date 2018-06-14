using System;
using UIKit;

namespace Toggl.Daneel.Cells
{
    public abstract class BaseTableCell<T> : UITableViewCell
    {
        private T item;
        public T Item
        {
            get => item;
            set
            {
                item = value;
                UpdateView();
            }
        }

        protected BaseTableCell()
        {
        }

        protected BaseTableCell(IntPtr handle)
            : base(handle)
        {
        }

        protected abstract void UpdateView();
    }
}
