using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Tests.Sync.States
{
    public interface ITestModel : IIdentifiable, ILastChangedDatable, IDeletable, IDatabaseSyncable
    {
    }

    public interface IDatabaseTestModel : IDatabaseSyncable, ITestModel
    {
    }

    public interface IThreadSafeTestModel : IThreadSafeModel, IDatabaseTestModel
    {
    }

    public sealed class TestModel : IThreadSafeTestModel
    {
        public long Id { get; set; }
        
        public DateTimeOffset At { get; set; }
        
        public DateTimeOffset? ServerDeletedAt { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public string LastSyncErrorMessage { get; set; }

        public bool IsDeleted { get; set; }

        public TestModel()
        {
        }

        public TestModel(long id, SyncStatus status, bool deleted = false)
        {
            Id = id;
            SyncStatus = status;
            IsDeleted = deleted;
        }

        public static TestModel From(ITestModel model)
            => new TestModel
            {
                Id = model.Id,
                At = model.At,
                IsDeleted = model.IsDeleted,
                ServerDeletedAt = model.ServerDeletedAt,
                SyncStatus = model.SyncStatus,
                LastSyncErrorMessage = model.LastSyncErrorMessage
            };

        public static TestModel Clean(ITestModel testModel)
            => new TestModel(testModel.Id, SyncStatus.InSync) { At = testModel.At };

        public static TestModel Dirty(long id)
            => new TestModel(id, SyncStatus.SyncNeeded);

        public static TestModel DirtyDeleted(long id)
            => new TestModel(id, SyncStatus.SyncNeeded, true);

        public static TestModel Unsyncable(ITestModel model, string message)
            => new TestModel { Id = model.Id, LastSyncErrorMessage = message, SyncStatus = SyncStatus.SyncFailed };
    }
}
