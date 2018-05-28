using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.States.Push.Interfaces;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Push
{
    public abstract class BasePushEntityState<TDatabaseModel, TThreadsafeModel> : IPushEntityState<TThreadsafeModel>
        where TThreadsafeModel : IThreadSafeModel, TDatabaseModel
    {
        private readonly IBaseDataSource<TThreadsafeModel, TDatabaseModel> dataSource;

        public StateResult<(Exception, TThreadsafeModel)> ServerError { get; } = new StateResult<(Exception, TThreadsafeModel)>();

        public StateResult<(Exception, TThreadsafeModel)> ClientError { get; } = new StateResult<(Exception, TThreadsafeModel)>();

        public StateResult<(Exception, TThreadsafeModel)> UnknownError { get; } = new StateResult<(Exception, TThreadsafeModel)>();

        protected BasePushEntityState(IBaseDataSource<TThreadsafeModel, TDatabaseModel> dataSource)
        {
            this.dataSource = dataSource;
        }

        protected Func<TThreadsafeModel, IObservable<TThreadsafeModel>> Overwrite(TThreadsafeModel entity)
            => pushedEntity => dataSource.Overwrite(entity, pushedEntity);

        protected Func<Exception, IObservable<ITransition>> Fail(TThreadsafeModel entity)
            => exception => shouldRethrow(exception)
                ? Observable.Throw<ITransition>(exception)
                : Observable.Return(failTransition(entity, exception));

        private bool shouldRethrow(Exception e)
            => e is ApiDeprecatedException || e is ClientDeprecatedException || e is UnauthorizedException || e is OfflineException;

        private ITransition failTransition(TThreadsafeModel entity, Exception e)
            => e is ServerErrorException
                ? ServerError.Transition((e, entity))
                : e is ClientErrorException
                    ? ClientError.Transition((e, entity))
                    : UnknownError.Transition((e, entity));

        public abstract IObservable<ITransition> Start(TThreadsafeModel entity);
    }
}
