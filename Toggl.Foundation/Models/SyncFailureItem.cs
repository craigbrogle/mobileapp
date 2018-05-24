using System;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Models
{
    public class SyncFailureItem
    {
        public SyncStatus SyncStatus { get; }

        public string SyncErrorMessage { get; }

        public ItemType Type { get; }

        public string Name { get; }

        public SyncFailureItem(IDatabaseSyncable databaseModel)
        {
            Ensure.Argument.IsNotNull(databaseModel, nameof(databaseModel));

            SyncStatus = databaseModel.SyncStatus;
            SyncErrorMessage = databaseModel.LastSyncErrorMessage;

            switch (databaseModel)
            {
                case IDatabaseTag tag:
                    Type = ItemType.Tag;
                    Name = tag.Name;
                    break;
                case IDatabaseClient client:
                    Type = ItemType.Client;
                    Name = client.Name;
                    break;
                case IDatabaseProject project:
                    Type = ItemType.Project;
                    Name = project.Name;
                    break;
                default:
                    throw new ArgumentException($"Unexpected type: {databaseModel.GetType()}");
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
