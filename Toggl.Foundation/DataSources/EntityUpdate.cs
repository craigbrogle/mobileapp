using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.DataSources
{
    public struct EntityUpdate<TThreadsafe>
        where TThreadsafe : IThreadSafeModel
    {
        public long Id { get; }
        
        public TThreadsafe Entity { get; }

        public EntityUpdate(long id, TThreadsafe entity)
        {
            Id = id;
            Entity = entity;
        }
    }
}
