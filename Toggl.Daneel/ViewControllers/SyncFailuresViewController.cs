using UIKit;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Daneel.Views.SyncFailures;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.Models;

namespace Toggl.Daneel.ViewControllers
{
    [MvxChildPresentation]
    public partial class SyncFailuresViewController : MvxViewController<SyncFailuresViewModel>
    {
        private UITableView FailuresTableView;

        public SyncFailuresViewController() : base()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            FailuresTableView = new UITableView(View.Frame, UITableViewStyle.Plain);
            View.Add(FailuresTableView);

            FailuresTableView.RowHeight = UITableView.AutomaticDimension;
            FailuresTableView.RegisterNibForCellReuse(SyncFailureCell.Nib, SyncFailureCell.Identifier);
            var source = new ListTableViewSource<SyncFailureItem, SyncFailureCell>(
                ViewModel.SyncFailures,
                SyncFailureCell.Identifier,
                cellConfiguration);

            FailuresTableView.Source = source;
        }

        private SyncFailureCell cellConfiguration(UITableViewCell cell, SyncFailureItem model)
        {
            var syncFailureCell = (SyncFailureCell)cell;
            syncFailureCell.Update(model);
            return syncFailureCell;
        }
    }
}
