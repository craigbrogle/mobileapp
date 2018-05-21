using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Suggestions;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SuggestionsViewModel : MvxViewModel
    {
        private readonly ITogglDataSource dataSource;
        private readonly IInteractorFactory interactorFactory;
        private readonly ISuggestionProviderContainer suggestionProviders;

        private bool areStartButtonsEnabled = true;
        private IDisposable emptyDatabaseDisposable;

        public MvxObservableCollection<Suggestion> Suggestions { get; }
            = new MvxObservableCollection<Suggestion>();

        public bool IsEmpty => !Suggestions.Any();

        public MvxAsyncCommand<Suggestion> StartTimeEntryCommand { get; set; }

        public SuggestionsViewModel(
            ITogglDataSource dataSource,
            IInteractorFactory interactorFactory,
            ISuggestionProviderContainer suggestionProviders)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(suggestionProviders, nameof(suggestionProviders));

            this.dataSource = dataSource;
            this.interactorFactory = interactorFactory;
            this.suggestionProviders = suggestionProviders;

            StartTimeEntryCommand = new MvxAsyncCommand<Suggestion>(startTimeEntry, _ => areStartButtonsEnabled);
        }

        public async override Task Initialize()
        {
            await base.Initialize();

            emptyDatabaseDisposable = dataSource
                .TimeEntries
                .IsEmpty
                .FirstAsync()
                .Subscribe(fetchSuggestions);
        }

        private void fetchSuggestions(bool databaseIsEmpty)
        {
            Suggestions.Clear();

            if (databaseIsEmpty) return;

            suggestionProviders
                .Providers
                .Select(provider => provider.GetSuggestions())
                .Aggregate(Observable.Merge)
                .Subscribe(addSuggestions);
        }

        private void addSuggestions(Suggestion suggestions)
        {
            Suggestions.Add(suggestions);

            RaisePropertyChanged();
        }

        private async Task startTimeEntry(Suggestion suggestion)
        {
            areStartButtonsEnabled = false;
            StartTimeEntryCommand.RaiseCanExecuteChanged();

            await interactorFactory
                .StartSuggestion(suggestion)
                .Execute()
                .Do(_ =>
                {
                    areStartButtonsEnabled = true;
                    StartTimeEntryCommand.RaiseCanExecuteChanged();
                });
        }

        public override void ViewDestroy()
        {
            base.ViewDestroy();
            emptyDatabaseDisposable?.Dispose();
        }
    }
}
