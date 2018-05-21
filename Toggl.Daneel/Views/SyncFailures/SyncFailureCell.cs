using System;
using Foundation;
using UIKit;
using Toggl.Foundation.Models;
using static Toggl.PrimeRadiant.SyncStatus;

namespace Toggl.Daneel.Views.SyncFailures
{
    public partial class SyncFailureCell : UITableViewCell
    {
        public static readonly string Identifier = "syncFailureCell";

        public static readonly NSString Key = new NSString("SyncFailureCell");
        public static readonly UINib Nib;

        static SyncFailureCell()
        {
            Nib = UINib.FromName("SyncFailureCell", NSBundle.MainBundle);
        }

        protected SyncFailureCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public void Update(SyncFailureItem model)
        {
            typeLabel.Text = $"{model.Type}: ";
            nameLabel.Text = model.Name;
            syncStatusLabel.Text = model.SyncStatus.ToString();
            errorMessageLabel.Text = model.SyncErrorMessage;
            switch (model.SyncStatus) {
                case SyncFailed:
                    syncStatusLabel.TextColor = UIColor.Red;
                    break;
                case SyncNeeded:
                    syncStatusLabel.TextColor = UIColor.DarkGray;
                    break;
                default:
                    break;
            }
        }
    }
}
