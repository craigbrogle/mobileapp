using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Fragments
{
    [MvxDialogFragmentPresentation(AddToBackStack = true)]
    public sealed class TermsOfServiceFragment : MvxDialogFragment<TermsOfServiceViewModel>
    {
        public TermsOfServiceFragment() { }

        public TermsOfServiceFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.TermsOfServiceFragment, null);
            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            Dialog.Window.SetDefaultDialogLayout(Activity, Context, heightDp: 350);
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            ViewModel.CloseCommand.ExecuteAsync();
        }
    }
}
