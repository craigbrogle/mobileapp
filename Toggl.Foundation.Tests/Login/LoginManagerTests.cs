using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Login;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;
using Xunit;
using FoundationUser = Toggl.Foundation.Models.User;
using User = Toggl.Ultrawave.Models.User;

namespace Toggl.Foundation.Tests.Login
{
    public sealed class LoginManagerTests
    {
        public abstract class LoginManagerTest
        {
            protected static readonly Password Password = "theirobotmoviesucked123".ToPassword();
            protected static readonly Email Email = "susancalvin@psychohistorian.museum".ToEmail();
            protected static readonly bool TermsAccepted = true;
            protected static readonly int CountryId = 237;

            protected IUser User { get; } = new User { Id = 10, ApiToken = "ABCDEFG" };
            protected ITogglApi Api { get; } = Substitute.For<ITogglApi>();
            protected IApiFactory ApiFactory { get; } = Substitute.For<IApiFactory>();
            protected ITogglDatabase Database { get; } = Substitute.For<ITogglDatabase>();
            protected IGoogleService GoogleService { get; } = Substitute.For<IGoogleService>();
            protected IAccessRestrictionStorage AccessRestrictionStorage { get; } = Substitute.For<IAccessRestrictionStorage>();
            protected ITogglDataSource DataSource { get; } = Substitute.For<ITogglDataSource>();
            protected IApplicationShortcutCreator ApplicationShortcutCreator { get; } = Substitute.For<IApplicationShortcutCreator>();

            protected readonly ILoginManager LoginManager;

            protected ITogglDataSource CreateDataSource(ITogglApi api) => DataSource;

            protected LoginManagerTest()
            {
                LoginManager = new LoginManager(ApiFactory, Database, GoogleService, ApplicationShortcutCreator, AccessRestrictionStorage, CreateDataSource);

                Api.User.Get().Returns(Observable.Return(User));
                Api.User.SignUp(Email, Password, TermsAccepted, CountryId).Returns(Observable.Return(User));
                Api.User.GetWithGoogle().Returns(Observable.Return(User));
                ApiFactory.CreateApiWith(Arg.Any<Credentials>()).Returns(Api);
                Database.Clear().Returns(Observable.Return(Unit.Default));
            }
        }

        public sealed class Constructor : LoginManagerTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(SixParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useApiFactory,
                bool useDatabase,
                bool useGoogleService,
                bool useAccessRestrictionStorage,
                bool useApplicationShortcutCreator,
                bool useCreateDataSource)
            {
                var database = useDatabase ? Database : null;
                var apiFactory = useApiFactory ? ApiFactory : null;
                var googleService = useGoogleService ? GoogleService : null;
                var accessRestrictionStorage = useAccessRestrictionStorage ? AccessRestrictionStorage : null;
                var createDataSource = useCreateDataSource ? CreateDataSource : (Func<ITogglApi, ITogglDataSource>)null;
                var shortcutCreator = useApplicationShortcutCreator ? ApplicationShortcutCreator : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new LoginManager(apiFactory, database, googleService, shortcutCreator, accessRestrictionStorage, createDataSource);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheLoginMethod : LoginManagerTest
        {
            [Theory, LogIfTooSlow]
            [InlineData("susancalvin@psychohistorian.museum", null)]
            [InlineData("susancalvin@psychohistorian.museum", "")]
            [InlineData("susancalvin@psychohistorian.museum", " ")]
            [InlineData("susancalvin@", null)]
            [InlineData("susancalvin@", "")]
            [InlineData("susancalvin@", " ")]
            [InlineData("susancalvin@", "123456")]
            [InlineData("", null)]
            [InlineData("", "")]
            [InlineData("", " ")]
            [InlineData("", "123456")]
            [InlineData(null, null)]
            [InlineData(null, "")]
            [InlineData(null, " ")]
            [InlineData(null, "123456")]
            public void ThrowsIfYouPassInvalidParameters(string email, string password)
            {
                var actualEmail = email.ToEmail();
                var actualPassword = password.ToPassword();

                Action tryingToConstructWithEmptyParameters =
                    () => LoginManager.Login(actualEmail, actualPassword).Wait();

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentException>();
            }

            [Fact, LogIfTooSlow]
            public async Task EmptiesTheDatabaseBeforeTryingToLogin()
            {
                await LoginManager.Login(Email, Password);

                Received.InOrder(async () =>
                {
                    await Database.Clear();
                    await Api.User.Get();
                });
            }

            [Fact, LogIfTooSlow]
            public async Task CallsTheGetMethodOfTheUserApi()
            {
                await LoginManager.Login(Email, Password);

                await Api.User.Received().Get();
            }

            [Fact, LogIfTooSlow]
            public async Task PersistsTheUserToTheDatabase()
            {
                await LoginManager.Login(Email, Password);

                await Database.User.Received().Create(Arg.Is<IDatabaseUser>(receivedUser => receivedUser.Id == User.Id));
            }

            [Fact, LogIfTooSlow]
            public async Task PersistsTheUserWithTheSyncStatusSetToInSync()
            {
                await LoginManager.Login(Email, Password);

                await Database.User.Received().Create(Arg.Is<IDatabaseUser>(receivedUser => receivedUser.SyncStatus == SyncStatus.InSync));
            }

            [Fact, LogIfTooSlow]
            public async Task AlwaysReturnsASingleResult()
            {
                await LoginManager
                        .Login(Email, Password)
                        .SingleAsync();
            }

            [Fact, LogIfTooSlow]
            public async Task NotifiesShortcutCreatorAboutLogin()
            {
                await LoginManager.Login(Email, Password);

                ApplicationShortcutCreator.Received().OnLogin(Arg.Any<ITogglDataSource>());
            }
        }

        public sealed class TheResetPasswordMethod : LoginManagerTest
        {
            [Theory, LogIfTooSlow]
            [InlineData("foo")]
            public void ThrowsWhenEmailIsInvalid(string emailString)
            {
                Action tryingToResetWithInvalidEmail = () => LoginManager
                    .ResetPassword(Email.From(emailString))
                    .Wait();

                tryingToResetWithInvalidEmail.Should().Throw<ArgumentException>();
            }

            [Fact, LogIfTooSlow]
            public async Task UsesApiWithoutCredentials()
            {
                await LoginManager.ResetPassword(Email.From("some@email.com"));

                ApiFactory.Received().CreateApiWith(Arg.Is<Credentials>(
                    arg => arg.Header.Name == null
                        && arg.Header.Value == null
                        && arg.Header.Type == HttpHeader.HeaderType.None));
            }

            [Theory, LogIfTooSlow]
            [InlineData("example@email.com")]
            [InlineData("john.smith@gmail.com")]
            [InlineData("h4cker123@domain.ru")]
            public async Task CallsApiClientWithThePassedEmailAddress(string address)
            {
                var email = address.ToEmail();

                await LoginManager.ResetPassword(email);

                await Api.User.Received().ResetPassword(Arg.Is(email));
            }
        }

        public sealed class TheGetDataSourceIfLoggedInInMethod : LoginManagerTest
        {
            [Fact, LogIfTooSlow]
            public void ReturnsNullIfTheDatabaseHasNoUsers()
            {
                var observable = Observable.Throw<IDatabaseUser>(new InvalidOperationException());
                Database.User.Single().Returns(observable);

                var result = LoginManager.GetDataSourceIfLoggedIn();

                result.Should().BeNull();
            }

            [Fact, LogIfTooSlow]
            public void ReturnsADataSourceIfTheUserExistsInTheDatabase()
            {
                var observable = Observable.Return<IDatabaseUser>(FoundationUser.Clean(User));
                Database.User.Single().Returns(observable);

                var result = LoginManager.GetDataSourceIfLoggedIn();

                result.Should().NotBeNull();
            }
        }

        public sealed class TheSignUpMethod : LoginManagerTest
        {
            [Theory, LogIfTooSlow]
            [InlineData("susancalvin@psychohistorian.museum", null)]
            [InlineData("susancalvin@psychohistorian.museum", "")]
            [InlineData("susancalvin@psychohistorian.museum", " ")]
            [InlineData("susancalvin@", null)]
            [InlineData("susancalvin@", "")]
            [InlineData("susancalvin@", " ")]
            [InlineData("susancalvin@", "123456")]
            [InlineData("", null)]
            [InlineData("", "")]
            [InlineData("", " ")]
            [InlineData("", "123456")]
            [InlineData(null, null)]
            [InlineData(null, "")]
            [InlineData(null, " ")]
            [InlineData(null, "123456")]
            public void ThrowsIfYouPassInvalidParameters(string email, string password)
            {
                var actualEmail = email.ToEmail();
                var actualPassword = password.ToPassword();

                Action tryingToConstructWithEmptyParameters =
                    () => LoginManager.SignUp(actualEmail, actualPassword, true, 0).Wait();

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentException>();
            }

            [Fact, LogIfTooSlow]
            public async Task EmptiesTheDatabaseBeforeTryingToCreateTheUser()
            {
                await LoginManager.SignUp(Email, Password, TermsAccepted, CountryId);

                Received.InOrder(async () =>
                {
                    await Database.Clear();
                    await Api.User.SignUp(Email, Password, TermsAccepted, CountryId);
                });
            }

            [Fact, LogIfTooSlow]
            public async Task CallsTheSignUpMethodOfTheUserApi()
            {
                await LoginManager.SignUp(Email, Password, TermsAccepted, CountryId);

                await Api.User.Received().SignUp(Email, Password, TermsAccepted, CountryId);
            }

            [Fact, LogIfTooSlow]
            public async Task PersistsTheUserToTheDatabase()
            {
                await LoginManager.SignUp(Email, Password, TermsAccepted, CountryId);

                await Database.User.Received().Create(Arg.Is<IDatabaseUser>(receivedUser => receivedUser.Id == User.Id));
            }

            [Fact, LogIfTooSlow]
            public async Task PersistsTheUserWithTheSyncStatusSetToInSync()
            {
                await LoginManager.SignUp(Email, Password, TermsAccepted, CountryId);

                await Database.User.Received().Create(Arg.Is<IDatabaseUser>(receivedUser => receivedUser.SyncStatus == SyncStatus.InSync));
            }

            [Fact, LogIfTooSlow]
            public async Task AlwaysReturnsASingleResult()
            {
                await LoginManager
                        .SignUp(Email, Password, TermsAccepted, CountryId)
                        .SingleAsync();
            }

            [Fact, LogIfTooSlow]
            public async Task NotifiesShortcutCreatorAboutLogin()
            {
                await LoginManager.SignUp(Email, Password, TermsAccepted, CountryId);

                ApplicationShortcutCreator.Received().OnLogin(Arg.Any<ITogglDataSource>());
            }
        }
        
        public sealed class TheRefreshTokenMethod : LoginManagerTest
        {
            public TheRefreshTokenMethod()
            {
                var user = Substitute.For<IDatabaseUser>();
                user.Email.Returns(Email);
                Database.User.Single().Returns(Observable.Return(user));
            }
 
            [Theory, LogIfTooSlow]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void ThrowsIfYouPassInvalidParameters(string password)
            {
                Action tryingToRefreshWithInvalidParameters =
                    () => LoginManager.RefreshToken(password.ToPassword()).Wait();
 
                tryingToRefreshWithInvalidParameters
                    .Should().Throw<ArgumentException>();
            }
 
            [Fact, LogIfTooSlow]
            public async Task CallsTheGetMethodOfTheUserApi()
            {
                await LoginManager.RefreshToken(Password);
 
                 await Api.User.Received().Get();
            }
 
            [Fact, LogIfTooSlow]
            public async Task PersistsTheUserToTheDatabase()
            {
                await LoginManager.RefreshToken(Password);
 
                await Database.User.Received().Update(Arg.Is<IDatabaseUser>(receivedUser => receivedUser.Id == User.Id));
            }
 
            [Fact, LogIfTooSlow]
            public async Task PersistsTheUserWithTheSyncStatusSetToInSync()
            {
                await LoginManager.RefreshToken(Password);
 
                await Database.User.Received().Update(Arg.Is<IDatabaseUser>(receivedUser => receivedUser.SyncStatus == SyncStatus.InSync));
            }
 
            [Fact, LogIfTooSlow]
            public async Task AlwaysReturnsASingleResult()
            {
                await LoginManager
                        .RefreshToken(Password)
                        .SingleAsync();
            }
        }

        public sealed class TheLoginUsingGoogleMethod : LoginManagerTest
        {
            public TheLoginUsingGoogleMethod()
            {
                GoogleService.GetAuthToken().Returns(Observable.Return("sometoken"));
            }

            [Fact, LogIfTooSlow]
            public async Task EmptiesTheDatabaseBeforeTryingToCreateTheUser()
            {
                await LoginManager.LoginWithGoogle();

                Received.InOrder(async () =>
                {
                    await Database.Clear();
                    await Api.User.GetWithGoogle();
                });
            }

            [Fact, LogIfTooSlow]
            public async Task UsesTheGoogleServiceToGetTheToken()
            {
                await LoginManager.LoginWithGoogle();

                await GoogleService.Received().GetAuthToken();
            }

            [Fact, LogIfTooSlow]
            public async Task CallsTheGetWithGoogleOfTheUserApi()
            {
                await LoginManager.LoginWithGoogle();

                await Api.User.Received().GetWithGoogle();
            }

            [Fact, LogIfTooSlow]
            public async Task PersistsTheUserToTheDatabase()
            {
                await LoginManager.LoginWithGoogle();

                await Database.User.Received().Create(Arg.Is<IDatabaseUser>(receivedUser => receivedUser.Id == User.Id));
            }

            [Fact, LogIfTooSlow]
            public async Task PersistsTheUserWithTheSyncStatusSetToInSync()
            {
                await LoginManager.LoginWithGoogle();

                await Database.User.Received().Create(Arg.Is<IDatabaseUser>(receivedUser => receivedUser.SyncStatus == SyncStatus.InSync));
            }

            [Fact, LogIfTooSlow]
            public async Task AlwaysReturnsASingleResult()
            {
                await LoginManager
                        .LoginWithGoogle()
                        .SingleAsync();
            }

            [Fact, LogIfTooSlow]
            public async Task NotifiesShortcutCreatorAboutLogin()
            {
                await LoginManager.LoginWithGoogle();

                ApplicationShortcutCreator.Received().OnLogin(Arg.Any<ITogglDataSource>());
            }
        }
    }
}
