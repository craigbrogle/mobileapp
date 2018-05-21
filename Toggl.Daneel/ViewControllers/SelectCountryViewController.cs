using MvvmCross.Binding.BindingContext;
using Toggl.Daneel.ViewSources;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using Toggl.Daneel.Extensions;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public sealed partial class SelectCountryViewController : KeyboardAwareViewController<SelectCountryViewModel>
    {
        public SelectCountryViewController() : base(nameof(SelectCountryViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new CountryTableViewSource(CountriesTableView);
            CountriesTableView.Source = source;

            var bindingSet = this.CreateBindingSet<SelectCountryViewController, SelectCountryViewModel>();

            bindingSet.Bind(source).To(vm => vm.Suggestions);
            bindingSet.Bind(SearchTextField).To(vm => vm.Text);
            bindingSet.Bind(CloseButton).To(vm => vm.CloseCommand);
            bindingSet.Bind(source)
                      .For(v => v.SelectionChangedCommand)
                      .To(vm => vm.SelectCountryCommand);

            bindingSet.Apply();

            SearchTextField.BecomeFirstResponder();
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            BottomConstraint.AnimateSetConstant(e.FrameEnd.Height, View);
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            BottomConstraint.AnimateSetConstant(0, View);
        }
    }
}

