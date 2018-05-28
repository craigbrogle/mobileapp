using System;
using System.Collections.Generic;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetAllNonDeletedInteractor : IInteractor<IObservable<IEnumerable<IThreadSafeTimeEntry>>>
    {
        private readonly IDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry> dataSource;

        public GetAllNonDeletedInteractor(IDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry> dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            
            this.dataSource = dataSource;
        }
        
        public IObservable<IEnumerable<IThreadSafeTimeEntry>> Execute()
            => dataSource.GetAll(te => !te.IsDeleted);
    }
}
