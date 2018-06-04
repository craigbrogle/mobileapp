﻿using System;
using Toggl.Foundation.DataSources;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using static Toggl.Multivac.WorkspaceFeatureId;

namespace Toggl.Foundation.Interactors
{
    internal sealed class AreCustomColorsEnabledForWorkspaceInteractor : WorkspaceHasFeatureInteractor<bool>
    {
        private readonly long workspaceId;

        public AreCustomColorsEnabledForWorkspaceInteractor(ITogglDataSource dataSource, long workspaceId)
            : base(dataSource)
        {
            this.workspaceId = workspaceId;
        }

        public override IObservable<bool> Execute()
            => CheckIfFeatureIsEnabled(workspaceId, Pro);
    }
}
