using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Toggl.Multivac.Extensions;
using UIKit;

namespace Toggl.Daneel.Extensions
{
    public static class UIExtensions
    {
        public static IObservable<Unit> Tapped(this UIButton button)
            => Observable
                .FromEventPattern(e => button.TouchUpInside += e, e => button.TouchUpInside -= e)
                .SelectUnit();

        public static IObservable<Unit> Tapped(this UIView view)
            => Observable.Create<Unit>(observer =>
            {
                var gestureRecognizer = new UITapGestureRecognizer(() => observer.OnNext(Unit.Default));
                view.AddGestureRecognizer(gestureRecognizer);

                return Disposable.Create(() => view.RemoveGestureRecognizer(gestureRecognizer));
            });

        public static Action<bool> BindIsVisible(this UIView view)
            => isVisible => view.Hidden = !isVisible;

        public static Action<string> BindText(this UILabel label)
            => text => label.Text = text;

        public static Action<string> BindText(this UITextView textView)
            => text => textView.Text = text;

        public static Action<string> BindText(this UITextField textField)
            => text => textField.Text = text;

        public static Action<bool> BindIsOn(this UISwitch @switch)
            => isOn => @switch.SetState(isOn, true);
    }
}
