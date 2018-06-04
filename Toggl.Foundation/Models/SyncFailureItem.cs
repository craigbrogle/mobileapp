using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Models
{
    public class SyncFailureItem
    {
        public SyncStatus SyncStatus { get; }

        public string SyncErrorMessage { get; }

        public ItemType Type { get; }

        public string Name { get; }

        public SyncFailureItem(IDatabaseSyncable model)
        {
            Ensure.Argument.IsNotNull(model, nameof(model));

            SyncStatus = model.SyncStatus;
            SyncErrorMessage = model.LastSyncErrorMessage;

            switch (model)
            {
                case IThreadSafeTag tag:
                    Type = ItemType.Tag;
                    Name = tag.Name;
                    break;
                case IThreadSafeClient client:
                    Type = ItemType.Client;
                    Name = client.Name;
                    break;
                case IThreadSafeProject project:
                    Type = ItemType.Project;
                    Name = project.Name;
                    break;
                default:
                    throw new ArgumentException($"Unexpected type: {model.GetType()}");
            }
        }
    }

    public enum ItemType
    {
        Tag,
        Project,
        Client
    }
}
