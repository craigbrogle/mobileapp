using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models;
using Toggl.Foundation.Shortcuts;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Login
{
    public sealed class LoginManager : ILoginManager
    {
        private readonly IApiFactory apiFactory;
        private readonly ITogglDatabase database;
        private readonly IGoogleService googleService;
        private readonly IApplicationShortcutCreator shortcutCreator;
        private readonly IAccessRestrictionStorage accessRestrictionStorage;
        private readonly Func<ITogglApi, ITogglDataSource> createDataSource;

        public LoginManager(
            IApiFactory apiFactory,
            ITogglDatabase database,
            IGoogleService googleService,
            IApplicationShortcutCreator shortcutCreator,
            IAccessRestrictionStorage accessRestrictionStorage,
            Func<ITogglApi, ITogglDataSource> createDataSource)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));
            Ensure.Argument.IsNotNull(apiFactory, nameof(apiFactory));
            Ensure.Argument.IsNotNull(accessRestrictionStorage, nameof(accessRestrictionStorage));
            Ensure.Argument.IsNotNull(googleService, nameof(googleService));
            Ensure.Argument.IsNotNull(shortcutCreator, nameof(shortcutCreator));
            Ensure.Argument.IsNotNull(createDataSource, nameof(createDataSource));

            this.database = database;
            this.apiFactory = apiFactory;
            this.accessRestrictionStorage = accessRestrictionStorage;
            this.googleService = googleService;
            this.shortcutCreator = shortcutCreator;
            this.createDataSource = createDataSource;
        }

        public IObservable<ITogglDataSource> Login(Email email, Password password)
        {
            if (!email.IsValid)
                throw new ArgumentException($"A valid {nameof(email)} must be provided when trying to login");
            if (!password.IsValid)
                throw new ArgumentException($"A valid {nameof(password)} must be provided when trying to login");

            var credentials = Credentials.WithPassword(email, password);

            return database
                    .Clear()
                    .SelectMany(_ => apiFactory.CreateApiWith(credentials).User.Get())
                    .Select(User.Clean)
                    .SelectMany(database.User.Create)
                    .Select(dataSourceFromUser)
                    .Do(shortcutCreator.OnLogin);
        }

        public IObservable<ITogglDataSource> LoginWithGoogle()
            => database
                .Clear()
                .SelectMany(_ => googleService.GetAuthToken())
                .Select(Credentials.WithGoogleToken)
                .Select(apiFactory.CreateApiWith)
                .SelectMany(api => api.User.GetWithGoogle())
                .Select(User.Clean)
                .SelectMany(database.User.Create)
                .Select(dataSourceFromUser)
                .Do(shortcutCreator.OnLogin);

        public IObservable<ITogglDataSource> SignUp(Email email, Password password, bool termsAccepted, int? countryId)
        {
            if (!email.IsValid)
                throw new ArgumentException($"A valid {nameof(email)} must be provided when trying to signup");
            if (!password.IsValid)
                throw new ArgumentException($"A valid {nameof(password)} must be provided when trying to signup");

            return database
                    .Clear()
                    .SelectMany(_ => signUp(email, password, termsAccepted, countryId))
                    .Select(User.Clean)
                    .SelectMany(database.User.Create)
                    .Select(dataSourceFromUser)
                    .Do(shortcutCreator.OnLogin);
        }

        public IObservable<ITogglDataSource> SignUpWithGoogle()
            => database
                .Clear()
                .SelectMany(_ => googleService.GetAuthToken())
                .SelectMany(apiFactory.CreateApiWith(Credentials.None).User.SignUpWithGoogle)
                .Select(User.Clean)
                .SelectMany(database.User.Create)
                .Select(dataSourceFromUser)
                .Do(shortcutCreator.OnLogin);

        public IObservable<string> ResetPassword(Email email)
        {
            if (!email.IsValid)
                throw new ArgumentException($"A valid {nameof(email)} must be provided when trying to reset forgotten password.");

            var api = apiFactory.CreateApiWith(Credentials.None);
            return api.User.ResetPassword(email);
        }

        public ITogglDataSource GetDataSourceIfLoggedIn()
            => database.User
                .Single()
                .Select(dataSourceFromUser)
                .Catch(Observable.Return<ITogglDataSource>(null))
                .Do(shortcutCreator.OnLogin)
                .Wait();

        public IObservable<ITogglDataSource> RefreshToken(Password password)
        {
            if (!password.IsValid)
                throw new ArgumentException($"A valid {nameof(password)} must be provided when trying to refresh token");

            return database.User
                .Single()
                .Select(user => user.Email)
                .Select(email => Credentials.WithPassword(email, password))
                .Select(apiFactory.CreateApiWith)
                .SelectMany(api => api.User.Get())
                .Select(User.Clean)
                .SelectMany(database.User.Update)
                .Select(dataSourceFromUser)
                .Do(shortcutCreator.OnLogin);
        }

        private ITogglDataSource dataSourceFromUser(IUser user)
        {
            var newCredentials = Credentials.WithApiToken(user.ApiToken);
            var api = apiFactory.CreateApiWith(newCredentials);
            return createDataSource(api);
        }

        private IObservable<IUser> signUp(Email email, Password password, bool termsAccepted, int? countryId)
        {
            return apiFactory
                .CreateApiWith(Credentials.None)
                .User
                .SignUp(email, password, termsAccepted, countryId);
        }
    }
}
