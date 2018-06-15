using System;
using System.Collections.Generic;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Helpers;
using Toggl.Ultrawave.Models;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Serialization;

namespace Toggl.Ultrawave.ApiClients
{
    internal sealed class WorkspacesApi : BaseApi, IWorkspacesApi
    {
        private readonly WorkspaceEndpoints endPoints;

        private readonly IJsonSerializer serializer;

        public WorkspacesApi(Endpoints endPoints, IApiClient apiClient, IJsonSerializer serializer,
            Credentials credentials)
            : base(apiClient, serializer, credentials, endPoints.LoggedIn)
        {
            this.endPoints = endPoints.Workspaces;
            this.serializer = serializer;
        }

        public IObservable<List<IWorkspace>> GetAll()
            => CreateListObservable<Workspace, IWorkspace>(endPoints.Get, AuthHeader);

        public IObservable<IWorkspace> GetById(long id)
        {
            var endpoint = endPoints.GetById(id);
            var observable = CreateObservable<Workspace>(endpoint, AuthHeader);
            return observable;
        }

        public IObservable<IWorkspace> Create(string name)
        {
            var dto = new UserApi.WorkspaceParameters { Name = name, InitialPricingPlan = PricingPlans.Free };
            var json = serializer.Serialize(dto, SerializationReason.Post, features: null);

            return CreateObservable<Workspace>(endPoints.Post, AuthHeader, json);
        }
    }
}
