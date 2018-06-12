using System;
using FluentAssertions;
using MvvmCross.Core.Navigation;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.Services
{
    public sealed class ApiErrorHandlingServiceTests
    {
        public abstract class BaseApiErrorHandlingServiceTests
        {
            protected readonly IMvxNavigationService NavigationService;
            protected readonly IAccessRestrictionStorage AccessRestrictionStorage;
            protected readonly IApiErrorHandlingService ApiErrorHandlingService;
            protected readonly IDatabaseUser User;

            public BaseApiErrorHandlingServiceTests()
            {
                User = Substitute.For<IDatabaseUser>();
                var token = Guid.NewGuid().ToString();
                User.ApiToken.Returns(token);
                NavigationService = Substitute.For<IMvxNavigationService>();
                AccessRestrictionStorage = Substitute.For<IAccessRestrictionStorage>();
                ApiErrorHandlingService =
                    new ApiErrorHandlingService(NavigationService, AccessRestrictionStorage);
            }
        }

        public sealed class TheConstructor
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(TwoParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useNavigationService, bool useAccessRestrictionStorage)
            {
                var navigationService = useNavigationService ? Substitute.For<IMvxNavigationService>() : null;
                var accessRestrictionStorage = useAccessRestrictionStorage ? Substitute.For<IAccessRestrictionStorage>() : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new ApiErrorHandlingService(navigationService, accessRestrictionStorage);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheApiDeprecatedException : BaseApiErrorHandlingServiceTests
        {
            private ApiDeprecatedException exception => new ApiDeprecatedException(Substitute.For<IRequest>(), Substitute.For<IResponse>());

            [Fact, LogIfTooSlow]
            public void ReturnsTrueForApiDeprecatedException()
            {
                var result = ApiErrorHandlingService.TryHandleDeprecationError(exception);

                result.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void RestricsAccessForApiDeprecatedException()
            {
                ApiErrorHandlingService.TryHandleDeprecationError(exception);

                AccessRestrictionStorage.Received().SetApiOutdated();
            }

            [Fact, LogIfTooSlow]
            public void NavigatesToOutdatedAppViewModelForApiDeprecatedException()
            {
                ApiErrorHandlingService.TryHandleDeprecationError(exception);

                NavigationService.Received().Navigate<OutdatedAppViewModel>();
            }
        }

        public sealed class TheClientDeprecatedException : BaseApiErrorHandlingServiceTests
        {
            private ClientDeprecatedException exception => new ClientDeprecatedException(Substitute.For<IRequest>(), Substitute.For<IResponse>());

            [Fact, LogIfTooSlow]
            public void ReturnsTrueForClientDeprecatedException()
            {
                var result = ApiErrorHandlingService.TryHandleDeprecationError(exception);

                result.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void RestricsAccessForClientDeprecatedException()
            {
                ApiErrorHandlingService.TryHandleDeprecationError(exception);

                AccessRestrictionStorage.Received().SetClientOutdated();
            }

            [Fact, LogIfTooSlow]
            public void NavigatesToOutdatedAppViewModelForClientDeprecatedException()
            {
                ApiErrorHandlingService.TryHandleDeprecationError(exception);

                NavigationService.Received().Navigate<OutdatedAppViewModel>();
            }
        }

        public sealed class TheOtherExceptionsThanDeprecationExceptions : BaseApiErrorHandlingServiceTests
        {
            private Exception exception => new Exception();

            [Fact, LogIfTooSlow]
            public void ReturnsFalseForDifferentExceptions()
            {
                ApiErrorHandlingService.TryHandleDeprecationError(exception);

                NavigationService.DidNotReceive().Navigate<OutdatedAppViewModel>();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotRestrictAccessForDifferentExceptions()
            {
                ApiErrorHandlingService.TryHandleDeprecationError(exception);

                AccessRestrictionStorage.DidNotReceive().SetApiOutdated();
                AccessRestrictionStorage.DidNotReceive().SetClientOutdated();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotNavigateForDifferentExceptions()
            {
                ApiErrorHandlingService.TryHandleDeprecationError(exception);

                NavigationService.DidNotReceive().Navigate<OutdatedAppViewModel>();
            }
        }

        public sealed class TheUnauthorizedException : BaseApiErrorHandlingServiceTests
        {
            private UnauthorizedException exception => new UnauthorizedException(createRequest(), Substitute.For<IResponse>());

            private IRequest createRequest()
            {
                var request = Substitute.For<IRequest>();
                var headers = new[] { Credentials.WithApiToken(User.ApiToken).Header };
                request.Headers.Returns(headers);
                return request;
            }

            [Fact, LogIfTooSlow]
            public void ReturnsTrueForClientDeprecatedException()
            {
                var result = ApiErrorHandlingService.TryHandleUnauthorizedError(exception);

                result.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void RestricsAccessForClientDeprecatedException()
            {
                ApiErrorHandlingService.TryHandleUnauthorizedError(exception);

                AccessRestrictionStorage.Received().SetUnauthorizedAccess(Arg.Is(User.ApiToken));
            }

            [Fact, LogIfTooSlow]
            public void NavigatesToOutdatedAppViewModelForClientDeprecatedException()
            {
                ApiErrorHandlingService.TryHandleUnauthorizedError(exception);

                NavigationService.Received().Navigate<TokenResetViewModel>();
            }

            [Fact, LogIfTooSlow]
            internal void ReturnsTrueButDoesNotNavigateOrSetUnathorizedAccessFlagWhenTheApiTokenCannotBeExtractedFromTheRequest()
            {
                var request = Substitute.For<IRequest>();
                request.Headers.Returns(new HttpHeader[0]);
                var exceptionWithoutApiToken = new UnauthorizedException(request, Substitute.For<IResponse>());

                var handled = ApiErrorHandlingService.TryHandleUnauthorizedError(exceptionWithoutApiToken);

                handled.Should().BeTrue();
                NavigationService.DidNotReceive().Navigate<TokenResetViewModel>();
                AccessRestrictionStorage.DidNotReceive().SetUnauthorizedAccess(Arg.Any<string>());
            }
        }

        public sealed class TheOtherExceptionsThanUnauthorizedException : BaseApiErrorHandlingServiceTests
        {
            private Exception exception => new Exception();

            [Fact, LogIfTooSlow]
            public void ReturnsFalseForDifferentExceptions()
            {
                ApiErrorHandlingService.TryHandleUnauthorizedError(exception);

                NavigationService.DidNotReceive().Navigate<TokenResetViewModel>();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotRestrictAccessForDifferentExceptions()
            {
                ApiErrorHandlingService.TryHandleUnauthorizedError(exception);

                AccessRestrictionStorage.DidNotReceive().SetUnauthorizedAccess(Arg.Any<string>());
            }

            [Fact, LogIfTooSlow]
            public void DoesNotNavigateForDifferentExceptions()
            {
                ApiErrorHandlingService.TryHandleUnauthorizedError(exception);

                NavigationService.DidNotReceive().Navigate<TokenResetViewModel>();
            }
        }
    }
}
