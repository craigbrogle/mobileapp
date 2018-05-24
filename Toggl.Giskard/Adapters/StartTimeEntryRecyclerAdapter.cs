using System;
using System.Collections.Specialized;
using System.Linq;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Giskard.TemplateSelectors;
using Toggl.Giskard.Views;

namespace Toggl.Giskard.Adapters
{
    public sealed class StartTimeEntryRecyclerAdapter
        : CreateSuggestionGroupedTableViewSource<WorkspaceGroupedCollection<AutocompleteSuggestion>, AutocompleteSuggestion>
    {
        public bool UseGrouping { get; set; }

        public bool IsSuggestingProjects { get; set; }

        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand { get; set; }

        public StartTimeEntryRecyclerAdapter()
        {
        }

        public StartTimeEntryRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override int ItemCount
        {
            get
            {
                if (UseGrouping)
                    return base.ItemCount;

                return (Collection.FirstOrDefault()?.Count ?? 0)
                    + (IsSuggestingCreation ? 1 : 0);
            }
        }

        public override object GetItem(int viewPosition)
        {
            if (UseGrouping)
                return base.GetItem(viewPosition);

            if (IsSuggestingCreation && viewPosition == 0)
                return GetCreateSuggestionItem();

            var actualViewPosition = viewPosition - (IsSuggestingCreation ? 1 : 0);
            return Collection.First()[actualViewPosition];
        }

        protected override void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsSourceCollectionChanged(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override MvxObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>> Collection
            => ItemsSource as MvxObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>>;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType != StartTimeEntrySuggestionsTemplateSelector.ProjectSuggestion)
                return base.OnCreateViewHolder(parent, viewType);

            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var inflatedView = InflateViewForHolder(parent, viewType, itemBindingContext);
            var viewHolder = new SelectProjectWithExpandableTasksRecyclerViewHolder(
                inflatedView, itemBindingContext, Resource.Id.StartTimeEntryToggleTasksButton)
            {
                Click = ItemClick,
                LongClick = ItemLongClick,
                ToggleTasksCommand = ToggleTasksCommand
            };

            return viewHolder;
        }

        protected override int SuggestCreationViewType => StartTimeEntrySuggestionsTemplateSelector.CreateEntity;

        protected override object GetCreateSuggestionItem()
            => IsSuggestingProjects
                ? $"{Resources.CreateProject} \"{Text}\""
                : $"{Resources.CreateTag} \"{Text}\"";
    }
}
