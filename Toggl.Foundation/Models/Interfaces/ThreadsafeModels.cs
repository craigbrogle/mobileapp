using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Models.Interfaces
{
    public partial interface IThreadSafeClient
        : IThreadSafeModel, IDatabaseClient
    {
    }

    public partial interface IThreadSafePreferences
        : IThreadSafeModel, IDatabasePreferences
    {
    }

    public partial interface IThreadSafeProject
        : IThreadSafeModel, IDatabaseProject
    {
    }

    public partial interface IThreadSafeTag
        : IThreadSafeModel, IDatabaseTag
    {
    }

    public partial interface IThreadSafeTask
        : IThreadSafeModel, IDatabaseTask
    {
    }

    public partial interface IThreadSafeTimeEntry
        : IThreadSafeModel, IDatabaseTimeEntry
    {
    }

    public partial interface IThreadSafeUser
        : IThreadSafeModel, IDatabaseUser
    {
    }

    public partial interface IThreadSafeWorkspace
        : IThreadSafeModel, IDatabaseWorkspace
    {
    }

    public partial interface IThreadSafeWorkspaceFeature
        : IThreadSafeModel, IDatabaseWorkspaceFeature
    {
    }

    public partial interface IThreadSafeWorkspaceFeatureCollection
        : IThreadSafeModel, IDatabaseWorkspaceFeatureCollection
    {
    }
}
