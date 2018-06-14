using MvvmCross.Core.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    public abstract class KeyboardAwareViewController<TViewModel> : ReactiveViewController<TViewModel>
        where TViewModel : class, IMvxViewModel
    {
        protected KeyboardAwareViewController(string nibName)
            : base(nibName) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UIKeyboard.Notifications.ObserveWillShow(KeyboardWillShow);
            UIKeyboard.Notifications.ObserveWillHide(KeyboardWillHide);
        }

        protected abstract void KeyboardWillShow(object sender, UIKeyboardEventArgs e);

        protected abstract void KeyboardWillHide(object sender, UIKeyboardEventArgs e);
    }
}
