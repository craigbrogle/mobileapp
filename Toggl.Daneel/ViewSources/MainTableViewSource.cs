using System;
using System.Collections;
using Foundation;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Core.ViewModels;
using Toggl.Daneel.Views;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Sync;
using Toggl.Foundation.MvvmCross.Collections;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class MainTableViewSource : MvxTableViewSource
    {
        public event EventHandler OnScrolled;

        public event EventHandler SwipeToContinueWasUsed
        {
            add { timeEntriesLogViewSource.SwipeToContinueWasUsed += value; }
            remove { timeEntriesLogViewSource.SwipeToContinueWasUsed -= value; }
        }

        private readonly TimeEntriesLogViewSource timeEntriesLogViewSource;
        private readonly SwipeToRefreshTableViewDelegate swipeToRefreshTableViewDelegate;

        public IMvxAsyncCommand<TimeEntryViewModel> ContinueTimeEntryCommand
        {
            get => timeEntriesLogViewSource.ContinueTimeEntryCommand;
            set => timeEntriesLogViewSource.ContinueTimeEntryCommand = value;
        }

        public NestableObservableCollection<TimeEntryViewModelCollection, TimeEntryViewModel> ObservableCollection
        {
            get => timeEntriesLogViewSource.ObservableCollection;
            set => timeEntriesLogViewSource.ObservableCollection = value;
        }

        public override IEnumerable ItemsSource
        {
            get => timeEntriesLogViewSource.ItemsSource;
            set { throw new InvalidOperationException($"You must bind to the {nameof(ObservableCollection)} and not the {nameof(ItemsSource)}"); }
        }

        public SyncProgress SyncProgress
        {
            get => swipeToRefreshTableViewDelegate.SyncProgress;
            set => swipeToRefreshTableViewDelegate.SyncProgress = value;
        }

        public IMvxCommand RefreshCommand
        {
            get => swipeToRefreshTableViewDelegate.RefreshCommand;
            set => swipeToRefreshTableViewDelegate.RefreshCommand = value;
        }

        public IMvxAsyncCommand<TimeEntryViewModel> DeleteTimeEntryCommand
        {
            get => timeEntriesLogViewSource.DeleteTimeEntryCommand;
            set => timeEntriesLogViewSource.DeleteTimeEntryCommand = value;
        }

        public new IMvxCommand<TimeEntryViewModel> SelectionChangedCommand
        {
            get => timeEntriesLogViewSource.SelectionChangedCommand;
            set => timeEntriesLogViewSource.SelectionChangedCommand = value;
        }

        public bool IsEmptyState
        {
            get => timeEntriesLogViewSource.IsEmptyState;
            set => timeEntriesLogViewSource.IsEmptyState = value;
        }

        public IObservable<TimeEntriesLogViewCell> FirstTimeEntry => timeEntriesLogViewSource.FirstTimeEntry;

        public MainTableViewSource(UITableView tableView) : base(tableView)
        {
            timeEntriesLogViewSource = new TimeEntriesLogViewSource(tableView);
            swipeToRefreshTableViewDelegate = new SwipeToRefreshTableViewDelegate(tableView);
        }

        public void Initialize()
        {
            swipeToRefreshTableViewDelegate.Initialize();
        }

        protected override object GetItemAt(NSIndexPath indexPath)
            => timeEntriesLogViewSource.GetItemAt(indexPath);

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => timeEntriesLogViewSource.GetOrCreateCellFor(tableView, indexPath, item);

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            => timeEntriesLogViewSource.GetCell(tableView, indexPath);

        public override UIView GetViewForFooter(UITableView tableView, nint section)
            => timeEntriesLogViewSource.GetViewForFooter(tableView, section);

        public override UIView GetViewForHeader(UITableView tableView, nint section)
            => timeEntriesLogViewSource.GetViewForHeader(tableView, section);

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
            => timeEntriesLogViewSource.GetHeightForHeader(tableView, section);

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
            => timeEntriesLogViewSource.GetHeightForFooter(tableView, section);

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            => timeEntriesLogViewSource.GetHeightForRow(tableView, indexPath);

        public override bool ShouldScrollToTop(UIScrollView scrollView)
            => timeEntriesLogViewSource.ShouldScrollToTop(scrollView);

        public override nint NumberOfSections(UITableView tableView)
            => timeEntriesLogViewSource.NumberOfSections(tableView);

        public override nint RowsInSection(UITableView tableview, nint section)
            => timeEntriesLogViewSource.RowsInSection(tableview, section);

        public override void HeaderViewDisplayingEnded(UITableView tableView, UIView headerView, nint section)
            => timeEntriesLogViewSource.HeaderViewDisplayingEnded(tableView, headerView, section);

        public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
            => timeEntriesLogViewSource.WillDisplayHeaderView(tableView, headerView, section);

        public override void Scrolled(UIScrollView scrollView)
        {
            swipeToRefreshTableViewDelegate.Scrolled(scrollView);
            OnScrolled?.Invoke(scrollView, null);
        }

        public override void DraggingStarted(UIScrollView scrollView)
            => swipeToRefreshTableViewDelegate.DraggingStarted(scrollView);

        public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
            => swipeToRefreshTableViewDelegate.DraggingEnded(scrollView, willDecelerate);

        public override void DecelerationEnded(UIScrollView scrollView)
            => swipeToRefreshTableViewDelegate.DecelerationEnded(scrollView);

        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            => timeEntriesLogViewSource.EditActionsForRow(tableView, indexPath);

        public override UISwipeActionsConfiguration GetLeadingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
            => timeEntriesLogViewSource.GetLeadingSwipeActionsConfiguration(tableView, indexPath);

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            => timeEntriesLogViewSource.RowSelected(tableView, indexPath);

        public override bool ShouldHighlightRow(UITableView tableView, NSIndexPath rowIndexPath)
            => timeEntriesLogViewSource.ShouldHighlightRow(tableView, rowIndexPath);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            timeEntriesLogViewSource.Dispose();
            swipeToRefreshTableViewDelegate.Dispose();
        }
    }
}
