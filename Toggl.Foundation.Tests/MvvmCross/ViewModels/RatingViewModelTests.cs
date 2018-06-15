using System;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class RatingViewModelTests
    {
        public abstract class RatingViewModelTest : BaseViewModelTests<RatingViewModel>
        {
            protected override RatingViewModel CreateViewModel()
                => new RatingViewModel(
                    DataSource,
                    RatingService,
                    FeedbackService,
                    AnalyticsService,
                    OnboardingStorage,
                    NavigationService);
        }

        public sealed class TheConstructor : RatingViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(SixParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useDataSource,
                bool useRatingService,
                bool useFeedbackService,
                bool useAnalyticsService,
                bool useOnboardingStorage,
                bool useNavigationService)
            {
                var dataSource = useDataSource ? DataSource : null;
                var ratingService = useRatingService ? RatingService : null;
                var feedbackService = useFeedbackService ? FeedbackService : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var navigationService = useNavigationService ? NavigationService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new RatingViewModel(
                        dataSource,
                        ratingService,
                        feedbackService,
                        analyticsService,
                        onboardingStorage,
                        navigationService);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheRegisterImpressionCommand : RatingViewModelTest
        {
            [Property]
            public void SetsTheGotImpressionPropertyToTrue(bool impressionIsPositive)
            {
                ViewModel.RegisterImpressionCommand.Execute(impressionIsPositive);

                ViewModel.GotImpression.Should().BeTrue();
            }

            [Property]
            public void TracksTheUserFinishedRatingViewFirstStepEvent(bool impressionIsPositive)
            {
                ViewModel.RegisterImpressionCommand.Execute(impressionIsPositive);

                AnalyticsService.UserFinishedRatingViewFirstStep.Received().Track(impressionIsPositive);
            }

            public abstract class RegisterImpressionCommandTest : RatingViewModelTest
            {
                protected abstract bool ImpressionIsPositive { get; }
                protected abstract string ExpectedCtaTitle { get; }
                protected abstract string ExpectedCtaDescription { get; }
                protected abstract string ExpectedCtaButtonTitle { get; }
                protected abstract RatingViewOutcome ExpectedStorageOucome { get; }

                [Fact, LogIfTooSlow]
                public void SetsTheAppropriateCtaTitle()
                {
                    ViewModel.RegisterImpressionCommand.Execute(ImpressionIsPositive);

                    ViewModel.CtaTitle.Should().Be(ExpectedCtaTitle);
                }

                [Fact, LogIfTooSlow]
                public void SetsTheAppropriateCtaDescription()
                {
                    ViewModel.RegisterImpressionCommand.Execute(ImpressionIsPositive);

                    ViewModel.CtaDescription.Should().Be(ExpectedCtaDescription);
                }

                [Fact, LogIfTooSlow]
                public void SetsTheAppropriateCtaButtonTitle()
                {
                    ViewModel.RegisterImpressionCommand.Execute(ImpressionIsPositive);

                    ViewModel.CtaButtonTitle.Should().Be(ExpectedCtaButtonTitle);
                }

                [Fact, LogIfTooSlow]
                public void StoresTheAppropriateRatingViewOutcome()
                {
                    ViewModel.RegisterImpressionCommand.Execute(ImpressionIsPositive);

                    OnboardingStorage.Received().SetRatingViewOutcome(ExpectedStorageOucome);
                }
            }

            public sealed class WhenImpressionIsPositive : RegisterImpressionCommandTest
            {
                protected override bool ImpressionIsPositive => true;
                protected override string ExpectedCtaTitle => Resources.RatingViewPositiveCTATitle;
                protected override string ExpectedCtaDescription => Resources.RatingViewPositiveCTADescription;
                protected override string ExpectedCtaButtonTitle => Resources.RatingViewPositiveCTAButtonTitle;
                protected override RatingViewOutcome ExpectedStorageOucome => RatingViewOutcome.PositiveImpression;
            }

            public sealed class WhenImpressionIsNegative : RegisterImpressionCommandTest
            {
                protected override bool ImpressionIsPositive => false;
                protected override string ExpectedCtaTitle => Resources.RatingViewNegativeCTATitle;
                protected override string ExpectedCtaDescription => Resources.RatingViewNegativeCTADescription;
                protected override string ExpectedCtaButtonTitle => Resources.RatingViewNegativeCTAButtonTitle;
                protected override RatingViewOutcome ExpectedStorageOucome => RatingViewOutcome.NegativeImpression;
            }
        }

        public sealed class ThePerformMainActionCommand
        {
            public abstract class PerformMainActionCommandTest : RatingViewModelTest
            {
                protected abstract void EnsureCorrectActionWasPerformed();
                protected abstract RatingViewSecondStepOutcome ExpectedEventParameterToTrack { get; }
                protected abstract RatingViewOutcome ExpectedStoragetOutcome { get; }

                [Fact, LogIfTooSlow]
                public void PerformsTheCorrectAction()
                {
                    ViewModel.PerformMainAction.Execute();

                    EnsureCorrectActionWasPerformed();
                }

                [Fact, LogIfTooSlow]
                public void TracksTheAppropriateEventWithTheExpectedParameter()
                {
                    ViewModel.PerformMainAction.Execute();

                    AnalyticsService
                        .UserFinishedRatingViewSecondStep
                        .Received()
                        .Track(ExpectedEventParameterToTrack);
                }

                [Fact, LogIfTooSlow]
                public void StoresTheAppropriateRatingViewOutcome()
                {
                    ViewModel.PerformMainAction.Execute();

                    OnboardingStorage
                        .Received()
                        .SetRatingViewOutcome(ExpectedStoragetOutcome);
                }
            }

            public sealed class WhenImpressionIsPositive : PerformMainActionCommandTest
            {
                protected override RatingViewOutcome ExpectedStoragetOutcome => RatingViewOutcome.AppWasRated;
                protected override RatingViewSecondStepOutcome ExpectedEventParameterToTrack => RatingViewSecondStepOutcome.AppWasRated;

                protected override void AdditionalViewModelSetup()
                {
                    ViewModel.RegisterImpressionCommand.Execute(true);
                }

                protected override void EnsureCorrectActionWasPerformed()
                {
                    RatingService.Received().AskForRating();
                }
            }

            public sealed class WhenImpressionIsNegative : PerformMainActionCommandTest
            {
                protected override RatingViewOutcome ExpectedStoragetOutcome => RatingViewOutcome.FeedbackWasLeft;
                protected override RatingViewSecondStepOutcome ExpectedEventParameterToTrack => RatingViewSecondStepOutcome.FeedbackWasLeft;

                protected override void AdditionalViewModelSetup()
                {
                    ViewModel.RegisterImpressionCommand.Execute(false);
                }

                protected override void EnsureCorrectActionWasPerformed()
                {
                    FeedbackService.Received().SubmitFeedback().Wait();
                }
            }

            public sealed class WhenImpressionWasntLeft : RatingViewModelTest
            {
                [Fact, LogIfTooSlow]
                public void DoesNothing()
                {
                    ViewModel.PerformMainAction.Execute();

                    RatingService.DidNotReceive().AskForRating();
                    FeedbackService.DidNotReceive().SubmitFeedback().Wait();
                }
            }
        }

        public sealed class TheDismissCommand : RatingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void HidesTheViewModel()
            {
                ViewModel.DismissViewCommand.Execute();

                NavigationService.Received().ChangePresentation(
                    Arg.Is<ToggleRatingViewVisibilityHint>(hint => hint.ForceHide == true)
                );
            }

            [Fact, LogIfTooSlow]
            public void DoesNotTrackAnythingIfImpressionWasNotLeft()
            {
                ViewModel.DismissViewCommand.Execute();

                AnalyticsService.UserFinishedRatingViewFirstStep.DidNotReceive().Track(Arg.Any<bool>());
                AnalyticsService.UserFinishedRatingViewSecondStep.DidNotReceive().Track(Arg.Any<RatingViewSecondStepOutcome>());
            }

            [Fact, LogIfTooSlow]
            public void DoesNotSotreAnythingIfImpressionWasNotLeft()
            {
                ViewModel.DismissViewCommand.Execute();

                OnboardingStorage.DidNotReceive().SetRatingViewOutcome(Arg.Any<RatingViewOutcome>());
            }

            public abstract class DismissCommandTest : RatingViewModelTest
            {
                protected abstract bool ImpressionIsPositive { get; }
                protected abstract RatingViewOutcome ExpectedStorageOutcome { get; }
                protected abstract RatingViewSecondStepOutcome ExpectedEventParameterToTrack { get; }

                protected override void AdditionalViewModelSetup()
                {
                    ViewModel.RegisterImpressionCommand.Execute(ImpressionIsPositive);
                }

                [Fact, LogIfTooSlow]
                public void StoresTheExpectedRatingViewOutcome()
                {
                    ViewModel.DismissViewCommand.Execute();

                    OnboardingStorage.Received().SetRatingViewOutcome(ExpectedStorageOutcome);
                }

                public void TracksTheAppropriateEventWithTheExpectedParameter()
                {
                    ViewModel.DismissViewCommand.Execute();

                    AnalyticsService.UserFinishedRatingViewSecondStep.Received().Track(ExpectedEventParameterToTrack);
                }
            }

            public sealed class WhenImpressionIsPositive : DismissCommandTest
            {
                protected override bool ImpressionIsPositive => true;
                protected override RatingViewOutcome ExpectedStorageOutcome => RatingViewOutcome.AppWasNotRated;
                protected override RatingViewSecondStepOutcome ExpectedEventParameterToTrack => RatingViewSecondStepOutcome.AppWasNotRated;
            }

            public sealed class WhenImpressionIsNegative : DismissCommandTest
            {
                protected override bool ImpressionIsPositive => false;
                protected override RatingViewOutcome ExpectedStorageOutcome => RatingViewOutcome.FeedbackWasNotLeft;
                protected override RatingViewSecondStepOutcome ExpectedEventParameterToTrack => RatingViewSecondStepOutcome.FeedbackWasNotLeft;
            }
        }
    }
}
