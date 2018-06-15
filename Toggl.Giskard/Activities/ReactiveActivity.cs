using System.Reactive.Disposables;
using Android.Content;
using Android.OS;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Droid.Support.V7.AppCompat.EventSource;
using MvvmCross.Droid.Views;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Giskard.Activities
{
    public abstract class ReactiveActivity<TViewModel> : MvxEventSourceAppCompatActivity, IMvxAndroidView, IReactiveBindingHolder
        where TViewModel : class, IMvxViewModel 
    {
        public CompositeDisposable DisposeBag { get; private set; } = new CompositeDisposable();

        protected abstract void InitializeViews();

        public object DataContext
        {
            get => BindingContext.DataContext;
            set => BindingContext.DataContext = value;
        }

        public TViewModel ViewModel
        {
            get => DataContext as TViewModel;
            set => DataContext = value;
        }

        IMvxViewModel IMvxView.ViewModel 
        { 
            get => ViewModel; 
            set => ViewModel = value as TViewModel;
        }

        public IMvxBindingContext BindingContext { get; set; }

        protected ReactiveActivity()
        {
            BindingContext = new MvxAndroidBindingContext(this, this);
            this.AddEventListeners();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ViewModel?.ViewCreated();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ViewModel?.ViewDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();
            ViewModel?.ViewAppearing();
        }

        protected override void OnResume()
        {
            base.OnResume();
            ViewModel?.ViewAppeared();
        }

        protected override void OnPause()
        {
            base.OnPause();
            ViewModel?.ViewDisappearing();
        }

        protected override void OnStop()
        {
            base.OnStop();
            ViewModel?.ViewDisappeared();
        }

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(MvxContextWrapper.Wrap(@base, this));
        }

        public void MvxInternalStartActivityForResult(Intent intent, int requestCode)
        {
            StartActivityForResult(intent, requestCode);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;
            DisposeBag?.Dispose();
        }
    }
}