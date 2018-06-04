using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Android.Support.V7.App;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.Droid.Platform;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Services;

namespace Toggl.Giskard.Services
{
    public sealed class DialogService : IDialogService
    {
        public IObservable<bool> Confirm(string title, string message, string confirmButtonText, string dismissButtonText)
        {
            var activity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

            return Observable.Create<bool>(observer =>
            {
                MvxSingleton<IMvxMainThreadDispatcher>.Instance.RequestMainThreadAction(() =>
                {
                    var builder = new AlertDialog.Builder(activity, Resource.Style.TogglDialog)
                        .SetMessage(message)
                        .SetPositiveButton(confirmButtonText, (s, e) =>
                        {
                            observer.OnNext(true);
                            observer.OnCompleted();
                        });

                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        builder = builder.SetTitle(title);
                    }

                    if (!string.IsNullOrEmpty(dismissButtonText))
                    {
                        builder = builder.SetNegativeButton(dismissButtonText, (s, e) =>
                        {
                            observer.OnNext(false);
                            observer.OnCompleted();
                        });
                    }

                    var dialog = builder.Create();
                    dialog.CancelEvent += (s, e) =>
                    {
                        observer.OnNext(false);
                        observer.OnCompleted();
                    };

                    dialog.Show();
                });

                return Disposable.Empty;
            });
        }

        public IObservable<Unit> Alert(string title, string message, string buttonTitle)
            => Confirm(title, message, buttonTitle, null).Select(_ => Unit.Default);

        public IObservable<bool> ConfirmDestructiveAction(ActionType type)
        {
            switch (type)
            {
                case ActionType.DiscardNewTimeEntry:
                    return Confirm(null, Resources.DiscardThisTimeEntry, Resources.Discard, Resources.Cancel);
                case ActionType.DiscardEditingChanges:
                    return Confirm(null, Resources.DiscardEditingChanges, Resources.Discard, Resources.ContinueEditing);
                case ActionType.DeleteExistingTimeEntry:
                    return Confirm(null, Resources.DeleteThisTimeEntry, Resources.Delete, Resources.Cancel);
            }

            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
