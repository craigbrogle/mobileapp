//using System;
//namespace Toggl.Foundation.Tests.MvvmCross.Services
//{
//    public sealed class FeedbackServiceTests
//    {
//        public sealed class TheSubmitFeedbackCommand : SettingsViewModelTest
//        {
//            [Property]
//            public void SendsAnEmailToTogglSupport(
//                NonEmptyString nonEmptyString0, NonEmptyString nonEmptyString1)
//            {
//                var phoneModel = nonEmptyString0.Get;
//                var os = nonEmptyString1.Get;
//                PlatformConstants.PhoneModel.Returns(phoneModel);
//                PlatformConstants.OperatingSystem.Returns(os);

//                ViewModel.SubmitFeedbackCommand.Execute();

//                MailService
//                    .Received()
//                    .Send(
//                        "support@toggl.com",
//                        Arg.Any<string>(),
//                        Arg.Any<string>())
//                    .Wait();
//            }

//            [Property]
//            public void SendsAnEmailWithTheProperSubject(
//                NonEmptyString nonEmptyString)
//            {
//                var subject = nonEmptyString.Get;
//                PlatformConstants.FeedbackEmailSubject.Returns(subject);

//                ViewModel.SubmitFeedbackCommand.ExecuteAsync().Wait();

//                MailService.Received()
//                    .Send(
//                        Arg.Any<string>(),
//                        subject,
//                        Arg.Any<string>())
//                   .Wait();
//            }

//            [Fact, LogIfTooSlow]
//            public async Task SendsAnEmailWithAppVersionPhoneModelAndOsVersion()
//            {
//                PlatformConstants.PhoneModel.Returns("iPhone Y");
//                PlatformConstants.OperatingSystem.Returns("iOS 4.2.0");
//                var expectedMessage = $"\n\nVersion: {UserAgent.ToString()}\nPhone: {PlatformConstants.PhoneModel}\nOS: {PlatformConstants.OperatingSystem}";

//                await ViewModel.SubmitFeedbackCommand.ExecuteAsync();

//                await MailService.Received().Send(
//                    Arg.Any<string>(),
//                    Arg.Any<string>(),
//                    expectedMessage);
//            }

//            [Property]
//            public void AlertsUserWhenMailServiceReturnsAnError(
//                NonEmptyString nonEmptyString0, NonEmptyString nonEmptyString1)
//            {
//                var errorTitle = nonEmptyString0.Get;
//                var errorMessage = nonEmptyString1.Get;
//                var result = new MailResult(false, errorTitle, errorMessage);
//                MailService
//                    .Send(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
//                    .Returns(Task.FromResult(result));

//                ViewModel.SubmitFeedbackCommand.Execute();

//                DialogService
//                    .Received()
//                    .Alert(errorTitle, errorMessage, Resources.Ok)
//                    .Wait();
//            }

//            [Theory, LogIfTooSlow]
//            [InlineData(true, "")]
//            [InlineData(true, "Error")]
//            [InlineData(true, null)]
//            [InlineData(false, "")]
//            [InlineData(false, null)]
//            public async Task DoesNotAlertUserWhenMailServiceReturnsSuccessOrDoesNotHaveErrorTitle(
//                bool success, string errorTitle)
//            {
//                var result = new MailResult(success, errorTitle, "");
//                MailService
//                    .Send(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
//                    .Returns(Task.FromResult(result));

//                await ViewModel.SubmitFeedbackCommand.ExecuteAsync();

//                await DialogService
//                    .DidNotReceive()
//                    .Alert(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
//            }
//        }
//    }
//}
