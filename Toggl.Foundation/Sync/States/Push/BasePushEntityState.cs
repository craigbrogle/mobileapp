using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.States.Push.Interfaces;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Push
{
    public abstract class BasePushEntityState<T> : IPushEntityState<T>
        where T : class, IThreadSafeModel
    {
        private readonly IBaseDataSource<T> dataSource;

        public StateResult<(Exception, T)> ServerError { get; } = new StateResult<(Exception, T)>();

        public StateResult<(Exception, T)> ClientError { get; } = new StateResult<(Exception, T)>();

        public StateResult<(Exception, T)> UnknownError { get; } = new StateResult<(Exception, T)>();

        protected BasePushEntityState(IBaseDataSource<T> dataSource)
        {
            this.dataSource = dataSource;
        }

        protected Func<T, IObservable<T>> Overwrite(T entity)
            => pushedEntity => dataSource.Overwrite(entity, pushedEntity);

        protected Func<Exception, IObservable<ITransition>> Fail(T entity)
            => exception => shouldRethrow(exception)
                ? Observable.Throw<ITransition>(exception)
                : Observable.Return(failTransition(entity, exception));

        private bool shouldRethrow(Exception e)
            => e is ApiDeprecatedException || e is ClientDeprecatedException || e is UnauthorizedException || e is OfflineException;

        private ITransition failTransition(T entity, Exception e)
            => e is ServerErrorException
                ? ServerError.Transition((e, entity))
                : e is ClientErrorException
                    ? ClientError.Transition((e, entity))
                    : UnknownError.Transition((e, entity));

        public abstract IObservable<ITransition> Start(T entity);
    }
}
