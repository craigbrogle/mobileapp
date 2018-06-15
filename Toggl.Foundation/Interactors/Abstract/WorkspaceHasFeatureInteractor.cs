using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Interactors
{
    internal abstract class WorkspaceHasFeatureInteractor<TValue> : IInteractor<IObservable<TValue>>
    {
        protected ITogglDataSource DataSource { get; }

        public WorkspaceHasFeatureInteractor(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            DataSource = dataSource;
        }

        public IObservable<bool> CheckIfFeatureIsEnabled(long workspaceId, WorkspaceFeatureId featureId)
            => DataSource.WorkspaceFeatures
                .GetById(workspaceId)
                .Select(featureCollection => featureCollection.IsEnabled(featureId));

        public abstract IObservable<TValue> Execute();
    }
}
