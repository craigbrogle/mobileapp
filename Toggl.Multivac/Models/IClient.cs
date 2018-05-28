namespace Toggl.Multivac.Models
{
    public interface IClient : IIdentifiable, IDeletable, ILastChangedDatable
    {
        long WorkspaceId { get; }

        string Name { get; }
    }
}
