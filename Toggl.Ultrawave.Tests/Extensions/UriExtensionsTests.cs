using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Toggl.Ultrawave.Extensions;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Tests.Extensions
{
    public sealed class UriExtensionsTests
    {
        private readonly Endpoints endpoints = new Endpoints(ApiEnvironment.Staging);

        [Property]
        public void RemovesIdFromTheUriWithoutATrailingSlash(PositiveInt id)
        {
            var uri = endpoints.Workspaces.GetById(id.Get).Url;

            var anonymizedUri = uri.Anonymize();

            anonymizedUri.ToString().Should().Be("https://mobile.toggl.space/api/v9/workspaces/{id}");
        }

        [Property]
        public void RemovesIdFromTheUriWithATrailingSlash(PositiveInt id)
        {
            var uri = endpoints.Projects.Post(id.Get).Url;

            var anonymizedUri = uri.Anonymize();

            anonymizedUri.ToString().Should().Be("https://mobile.toggl.space/api/v9/workspaces/{id}/projects");
        }

        [Property]
        public void DoesNotRemoveTimestampFromTheUrl(PositiveInt id)
        {
            var uri = endpoints.TimeEntries.GetSince(DateTimeOffset.Now).Url;

            var anonymizedUri = uri.Anonymize();

            anonymizedUri.ToString().Should().Be(uri.ToString());
        }

        [Property]
        public void DoesNotRemoveDatesFromTheUrl(PositiveInt id)
        {
            var uri = endpoints.TimeEntries.GetBetween(DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1)).Url;

            var anonymizedUri = uri.Anonymize();

            anonymizedUri.ToString().Should().Be(uri.ToString());
        }

        [Property]
        public void RemovesAnIdButKeepsTimestampIntact(PositiveInt id)
        {
            var date = new DateTimeOffset(2018, 01, 02, 03, 04, 05, TimeSpan.Zero);
            var uri = new Uri(new Uri("https://mobile.toggl.space/api/v9/"), $"workspaces/{id.Get}/something?since={date.ToUnixTimeSeconds()}");

            var anonymizedUri = uri.Anonymize();

            anonymizedUri.ToString().Should().Be($"https://mobile.toggl.space/api/v9/workspaces/{{id}}/something?since={date.ToUnixTimeSeconds()}");
        }
    }
}
