using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Settings;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class AboutViewModelTests
    {
        public abstract class AboutViewModelTest : BaseViewModelTests<AboutViewModel>
        {
            protected override AboutViewModel CreateViewModel()
                => new AboutViewModel(NavigationService);
        }

        public sealed class TheConstructor : AboutViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfTheArgumentIsNull()
            {
                Action tryingToConstructWithEmptyParameters =
                    () => new AboutViewModel(null);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheLicensesCommand : AboutViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheLicensesViewModel()
            {
                await ViewModel.OpenLicensesCommand.ExecuteAsync();

                await NavigationService.Received().Navigate<LicensesViewModel>();
            }
        }

        public sealed class TheTermsOfServiceCommand : AboutViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void OpensTheBrowserInTheTermsOfServicePage()
            {
                ViewModel.OpenTermsOfServiceCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Url == Resources.TermsOfServiceUrl)
                );
            }

            [Fact, LogIfTooSlow]
            public void OpensTheBrowserWithTheAppropriateTitle()
            {
                ViewModel.OpenTermsOfServiceCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Title == Resources.TermsOfService)
                );
            }
        }

        public sealed class ThePrivacyPolicyCommand : AboutViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void OpensTheBrowserInThePrivacyPolicyPage()
            {
                ViewModel.OpenPrivacyPolicyCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Url == Resources.PrivacyPolicyUrl)
                );
            }

            [Fact, LogIfTooSlow]
            public void OpensTheBrowserWithTheAppropriateTitle()
            {
                ViewModel.OpenPrivacyPolicyCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Title == Resources.PrivacyPolicy)
                );
            }
        }
    }
}
