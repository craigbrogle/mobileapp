using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class LicensesViewModelTests
    {
        public abstract class LicensesViewModelTest : BaseViewModelTests<LicensesViewModel>
        {
            protected override LicensesViewModel CreateViewModel()
                => new LicensesViewModel(LicenseProvider);
        }

        public sealed class TheConstructor : LicensesViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfTheArgumentIsNull()
            {
                Action tryingToConstructWithEmptyParameters =
                    () => new LicensesViewModel(null);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheInitializeMethod : LicensesViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task InitializesTheLicenses()
            {
                var licenses = new Dictionary<string, string>
                {
                    { "Something", "Some long license" },
                    { "Something else", "Some other license" },
                    { "Third one", "Another even longer license" }
                };
                var expectedLicenses = licenses
                    .Select(license => new License(license.Key, license.Value))
                    .ToList();
                LicenseProvider.GetAppLicenses().Returns(licenses);

                await ViewModel.Initialize();

                ViewModel.Licenses.Should().BeEquivalentTo(expectedLicenses);
            }
        }
    }
}
