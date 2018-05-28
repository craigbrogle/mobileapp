using System;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Helpers;
using Toggl.Ultrawave.Models;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Serialization;
using Toggl.Ultrawave.Serialization.Converters;

namespace Toggl.Ultrawave.ApiClients
{
    internal sealed class UserApi : BaseApi, IUserApi
    {
        private const string userAlreadyExistsApiErrorMessage = "user with this email already exists";

        private readonly UserEndpoints endPoints;
        private readonly IJsonSerializer serializer;

        public UserApi(Endpoints endPoints, IApiClient apiClient, IJsonSerializer serializer,
            Credentials credentials)
            : base(apiClient, serializer, credentials, endPoints.LoggedIn)
        {
            this.endPoints = endPoints.User;
            this.serializer = serializer;
        }

        public IObservable<IUser> Get()
            => CreateObservable<User>(endPoints.Get, AuthHeader, responseValidator: checkForApiToken);

        public IObservable<IUser> GetWithGoogle()
            => CreateObservable<User>(endPoints.GetWithGoogle, AuthHeader, responseValidator: checkForApiToken);

        public IObservable<IUser> Update(IUser user)
            => CreateObservable(endPoints.Put, AuthHeader, user as User ?? new User(user),
                SerializationReason.Post, responseValidator: checkForApiToken);

        public IObservable<string> ResetPassword(Email email)
        {
            var json = $"{{\"email\":\"{email}\"}}";
            return CreateObservable(endPoints.ResetPassword, new HttpHeader[0], json)
                .Select(instructions => instructions.Trim('"'));
        }

        public IObservable<IUser> SignUp(
            Email email,
            Password password,
            bool termsAccepted,
            int countryId
        )
        {
            if (!email.IsValid)
                throw new ArgumentException(nameof(email));

            var dto = new SignUpParameters
            {
                Email = email,
                Password = password,
                Workspace = new WorkspaceParameters
                {
                    InitialPricingPlan = PricingPlans.Free
                },
                TermsAccepted = termsAccepted,
                CountryId = countryId
            };
            var json = serializer.Serialize(dto, SerializationReason.Post, null);
            return CreateObservable<User>(endPoints.Post, new HttpHeader[0], json, checkForApiToken)
                .Catch<IUser, BadRequestException>(badRequestException
                    => badRequestException.LocalizedApiErrorMessage == userAlreadyExistsApiErrorMessage
                        ? Observable.Throw<IUser>(new EmailIsAlreadyUsedException(badRequestException))
                        : Observable.Throw<IUser>(badRequestException));
        }

        public IObservable<IUser> SignUpWithGoogle(string googleToken)
        {
            Ensure.Argument.IsNotNull(googleToken, nameof(googleToken));
            var parameters = new GoogleSignUpParameters
            {
                GoogleAccessToken = googleToken,
                Workspace = new WorkspaceParameters
                {
                    InitialPricingPlan = PricingPlans.Free
                }
            };

            var json = serializer.Serialize(parameters, SerializationReason.Post, null);
            return CreateObservable<User>(endPoints.PostWithGoogle, new HttpHeader[0], json, checkForApiToken);
        }

        private void checkForApiToken(IRequest request, IResponse response, User user)
        {
            if (string.IsNullOrWhiteSpace(user.ApiToken))
                throw new UserIsMissingApiTokenException(request, response);
        }

        [Preserve(AllMembers = true)]
        internal class WorkspaceParameters
        {
            public string Name { get; } = null;

            public PricingPlans InitialPricingPlan { get; set; }
        }

        [Preserve(AllMembers = true)]
        private class SignUpParameters
        {
            [JsonConverter(typeof(EmailConverter))]
            public Email Email { get; set; }

            [JsonConverter(typeof(PasswordConverter))]
            public Password Password { get; set; }

            public WorkspaceParameters Workspace { get; set; }

            [JsonProperty("tos_accepted")]
            public bool TermsAccepted { get; set; }

            public int CountryId { get; set; }
        }

        [Preserve(AllMembers = true)]
        private class GoogleSignUpParameters
        {
            public string GoogleAccessToken { get; set; }

            public WorkspaceParameters Workspace { get; set; }
        }
    }
}
