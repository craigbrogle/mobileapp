using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class FetchObservables : IFetchObservables
    {
        private readonly IObservable<IUser> user;

        private readonly IObservable<List<IWorkspace>> workspaces;

        private readonly IObservable<List<IWorkspaceFeatureCollection>> workspaceFeatures;

        private readonly IObservable<List<IClient>> clients;

        private readonly IObservable<List<IProject>> projects;

        private readonly IObservable<List<ITag>> tags;

        private readonly IObservable<List<ITask>> tasks;

        private readonly IObservable<List<ITimeEntry>> timeEntries;

        private readonly IObservable<IPreferences> preferences;

        public FetchObservables(
            IObservable<List<IWorkspace>> workspaces,
            IObservable<List<IWorkspaceFeatureCollection>> workspaceFeatures, 
            IObservable<IUser> user,
            IObservable<List<IClient>> clients,
            IObservable<List<IProject>> projects,
            IObservable<List<ITimeEntry>> timeEntries,
            IObservable<List<ITag>> tags,
            IObservable<List<ITask>> tasks,
            IObservable<IPreferences> preferences)
        {
            this.workspaces = workspaces;
            this.workspaceFeatures = workspaceFeatures;
            this.user = user;
            this.clients = clients;
            this.projects = projects;
            this.timeEntries = timeEntries;
            this.tags = tags;
            this.tasks = tasks;
            this.preferences = preferences;
        }

        public IObservable<List<T>> Get<T>()
        {
            if (typeof(T) == typeof(IWorkspace))
                return (IObservable<List<T>>)workspaces;

            if (typeof(T) == typeof(IWorkspaceFeatureCollection))
                return (IObservable<List<T>>)workspaceFeatures;

            if (typeof(T) == typeof(IClient))
                return (IObservable<List<T>>)clients;

            if (typeof(T) == typeof(IProject))
                return (IObservable<List<T>>)projects;

            if (typeof(T) == typeof(ITag))
                return (IObservable<List<T>>)tags;

            if (typeof(T) == typeof(ITask))
                return (IObservable<List<T>>)tasks;
            
            if (typeof(T) == typeof(IUser))
                return (IObservable<List<T>>)user.Select(fetchedUser => new List<IUser> { fetchedUser });

            if (typeof(T) == typeof(IPreferences))
                return (IObservable<List<T>>)preferences.Select(fetchedPreferences => new List<IPreferences> { fetchedPreferences });

            if (typeof(T) == typeof(ITimeEntry))
                return (IObservable<List<T>>)timeEntries;

            throw new ArgumentException($"Type {typeof(T).FullName} is not supported by the {nameof(FetchObservables)} class.");
        }
    }
}
