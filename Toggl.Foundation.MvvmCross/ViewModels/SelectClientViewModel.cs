using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using static Toggl.Foundation.Helper.Constants;
using static Toggl.Multivac.Extensions.StringExtensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectClientViewModel : MvxViewModel<SelectClientParameters, long?>
    {

        private readonly ITogglDataSource dataSource;
        private readonly IMvxNavigationService navigationService;

        private long workspaceId;
        private long selectedClientId;
        private SelectableClientViewModel noClient;
        private IEnumerable<IThreadSafeClient> allClients;

        public string Text { get; set; } = "";

        public bool SuggestCreation
        {
            get
            {
                var text = Text.Trim();
                return !string.IsNullOrEmpty(text)
                    && !Suggestions.Any(s => s.Name == text)
                    && text.LengthInBytes() <= MaxClientNameLengthInBytes;
            }
        }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand CreateClientCommand { get; }

        public IMvxAsyncCommand<string> SelectClientCommand { get; }

        public MvxObservableCollection<SelectableClientViewModel> Suggestions { get; }
            = new MvxObservableCollection<SelectableClientViewModel>();

        public SelectClientViewModel(ITogglDataSource dataSource, IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.dataSource = dataSource;
            this.navigationService = navigationService;

            CloseCommand = new MvxAsyncCommand(close);
            CreateClientCommand = new MvxAsyncCommand(createClient);
            SelectClientCommand = new MvxAsyncCommand<string>(selectClient);
        }

        public override void Prepare(SelectClientParameters parameter)
        {
            workspaceId = parameter.WorkspaceId;
            selectedClientId = parameter.SelectedClientId;
            noClient = new SelectableClientViewModel(Resources.NoClient, selectedClientId == 0);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            allClients = await dataSource.Clients.GetAllInWorkspace(workspaceId);

            Suggestions.Add(noClient);
            Suggestions.AddRange(allClients.Select(c => new SelectableClientViewModel(c.Name, c.Id == selectedClientId)));
        }

        private void OnTextChanged()
        {
            Suggestions.Clear();
            var text = Text.Trim();
            Suggestions.AddRange(
                allClients
                    .Where(c => c.Name.ContainsIgnoringCase(text))
                    .Select(c => new SelectableClientViewModel(c.Name, c.Id == selectedClientId))
            );

            if (!string.IsNullOrEmpty(Text)) return;
            Suggestions.Insert(0, noClient);
        }

        private Task close()
            => navigationService.Close(this, null);

        private async Task selectClient(string clientName)
        {
            var clientId = allClients.FirstOrDefault(c => c.Name == clientName)?.Id ?? 0;
            await navigationService.Close(this, clientId);
        }

        private async Task createClient()
        {
            if (!SuggestCreation) return;

            var client = await dataSource.Clients.Create(Text.Trim(), workspaceId);
            await navigationService.Close(this, client.Id);
        }
    }
}
