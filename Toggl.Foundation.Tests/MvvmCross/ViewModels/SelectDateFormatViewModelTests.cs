using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;
using Toggl.Multivac;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SelectDateFormatViewModelTests
    {
        public abstract class SelectDateFormatViewModelTest : BaseViewModelTests<SelectDateFormatViewModel>
        {
            protected override SelectDateFormatViewModel CreateViewModel()
                => new SelectDateFormatViewModel(NavigationService);
        }

        public sealed class TheConstructor
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfTheArgumentIsNull()
            {
                Action tryingToConstructWithEmptyParameter =
                    () => new SelectDateFormatViewModel(null);

                tryingToConstructWithEmptyParameter.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class ThePrepareMethod : SelectDateFormatViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void MarksTheSelectedDateFormatAsSelected()
            {
                var selectedDateFormat = ViewModel.DateTimeFormats[0];

                ViewModel.Prepare(selectedDateFormat.DateFormat);

                selectedDateFormat.Selected.Should().BeTrue();
            }
        }

        public sealed class TheCloseCommand : SelectDateFormatViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModelPassingTheDefaultResult()
            {
                var defaultResult = DateFormat.FromLocalizedDateFormat("YYYY.MM.DD");
                ViewModel.Prepare(defaultResult);

                await ViewModel.CloseCommand.ExecuteAsync();

                await NavigationService.Received().Close(ViewModel, defaultResult);
            }
        }

        public sealed class TheSelectFormatCommand : SelectDateFormatViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModelPassingTheSelectedResult()
            {
                var selectedDateFormat = DateFormat.FromLocalizedDateFormat("DD.MM.YYYY");
                var selectableDateFormatViewModel = new SelectableDateFormatViewModel(selectedDateFormat, false);

                await ViewModel.SelectFormatCommand.ExecuteAsync(selectableDateFormatViewModel);

                await NavigationService.Received().Close(ViewModel, selectedDateFormat);    
            }
        }
    }
}
