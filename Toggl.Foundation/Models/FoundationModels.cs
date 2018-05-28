﻿using System;
using System.Collections.Generic;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Models
{
    internal partial class Client : IThreadSafeClient
    {
        public long Id { get; }

        public long WorkspaceId { get; }

        public string Name { get; }

        public IDatabaseWorkspace Workspace { get; }

        public bool IsDeleted { get; }

        public SyncStatus SyncStatus { get; }

        public string LastSyncErrorMessage { get; }

        public DateTimeOffset At { get; }

        public DateTimeOffset? ServerDeletedAt { get; }
    }

    internal partial class Preferences : IThreadSafePreferences
    {
        public TimeFormat TimeOfDayFormat { get; }

        public DateFormat DateFormat { get; }

        public DurationFormat DurationFormat { get; }

        public bool CollapseTimeEntries { get; }

        public bool IsDeleted { get; }

        public SyncStatus SyncStatus { get; }

        public string LastSyncErrorMessage { get; }
    }

    internal partial class Project : IThreadSafeProject
    {
        public long Id { get; }

        public long WorkspaceId { get; }

        public long? ClientId { get; }

        public string Name { get; }

        public bool IsPrivate { get; }

        public bool Active { get; }

        public string Color { get; }

        public bool? Billable { get; }

        public bool? Template { get; }

        public bool? AutoEstimates { get; }

        public long? EstimatedHours { get; }

        public double? Rate { get; }

        public string Currency { get; }

        public int? ActualHours { get; }

        public IDatabaseClient Client { get; }

        public IDatabaseWorkspace Workspace { get; }

        public IEnumerable<IDatabaseTask> Tasks { get; }

        public bool IsDeleted { get; }

        public SyncStatus SyncStatus { get; }

        public string LastSyncErrorMessage { get; }

        public DateTimeOffset At { get; }

        public DateTimeOffset? ServerDeletedAt { get; }
    }

    internal partial class Tag : IThreadSafeTag
    {
        public long Id { get; }

        public long WorkspaceId { get; }

        public string Name { get; }

        public IDatabaseWorkspace Workspace { get; }

        public bool IsDeleted { get; }

        public SyncStatus SyncStatus { get; }

        public string LastSyncErrorMessage { get; }

        public DateTimeOffset At { get; }

        public DateTimeOffset? ServerDeletedAt { get; }
    }

    internal partial class Task : IThreadSafeTask
    {
        public long Id { get; }

        public string Name { get; }

        public long ProjectId { get; }

        public long WorkspaceId { get; }

        public long? UserId { get; }

        public long EstimatedSeconds { get; }

        public bool Active { get; }

        public long TrackedSeconds { get; }

        public IDatabaseUser User { get; }

        public IDatabaseProject Project { get; }

        public IDatabaseWorkspace Workspace { get; }

        public bool IsDeleted { get; }

        public SyncStatus SyncStatus { get; }

        public string LastSyncErrorMessage { get; }

        public DateTimeOffset At { get; }
    }

    internal partial class TimeEntry : IThreadSafeTimeEntry
    {
        public long Id { get; }

        public long WorkspaceId { get; }

        public long? ProjectId { get; }

        public long? TaskId { get; }

        public bool Billable { get; }

        public DateTimeOffset Start { get; }

        public long? Duration { get; }

        public string Description { get; }

        public IEnumerable<long> TagIds { get; }

        public long UserId { get; }

        public IDatabaseTask Task { get; }

        public IDatabaseUser User { get; }

        public IDatabaseProject Project { get; }

        public IDatabaseWorkspace Workspace { get; }

        public IEnumerable<IDatabaseTag> Tags { get; }

        public bool IsDeleted { get; }

        public SyncStatus SyncStatus { get; }

        public string LastSyncErrorMessage { get; }

        public DateTimeOffset At { get; }

        public DateTimeOffset? ServerDeletedAt { get; }
    }

    internal partial class User : IThreadSafeUser
    {
        public long Id { get; }

        public string ApiToken { get; }

        public long DefaultWorkspaceId { get; }

        public Email Email { get; }

        public string Fullname { get; }

        public BeginningOfWeek BeginningOfWeek { get; }

        public string Language { get; }

        public string ImageUrl { get; }

        public bool IsDeleted { get; }

        public SyncStatus SyncStatus { get; }

        public string LastSyncErrorMessage { get; }

        public DateTimeOffset At { get; }
    }

    internal partial class Workspace : IThreadSafeWorkspace
    {
        public long Id { get; }

        public string Name { get; }

        public bool Admin { get; }

        public DateTimeOffset? SuspendedAt { get; }

        public double? DefaultHourlyRate { get; }

        public string DefaultCurrency { get; }

        public bool OnlyAdminsMayCreateProjects { get; }

        public bool OnlyAdminsSeeBillableRates { get; }

        public bool OnlyAdminsSeeTeamDashboard { get; }

        public bool ProjectsBillableByDefault { get; }

        public int Rounding { get; }

        public int RoundingMinutes { get; }

        public string LogoUrl { get; }

        public bool IsDeleted { get; }

        public SyncStatus SyncStatus { get; }

        public string LastSyncErrorMessage { get; }

        public DateTimeOffset At { get; }

        public DateTimeOffset? ServerDeletedAt { get; }
    }

    internal partial class WorkspaceFeature : IThreadSafeWorkspaceFeature
    {
        public WorkspaceFeatureId FeatureId { get; }

        public bool Enabled { get; }

    }

    internal partial class WorkspaceFeatureCollection : IThreadSafeWorkspaceFeatureCollection
    {
        public long WorkspaceId { get; }

        public IEnumerable<IWorkspaceFeature> Features { get; }

        public IDatabaseWorkspace Workspace { get; }

        public IEnumerable<IDatabaseWorkspaceFeature> DatabaseFeatures { get; }

    }
}