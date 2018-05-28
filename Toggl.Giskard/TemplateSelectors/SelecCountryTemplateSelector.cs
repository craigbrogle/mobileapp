using System;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Collections;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SelectCountryTemplateSelector : MvxDefaultTemplateSelector
    {
        public SelectCountryTemplateSelector()
            : base(Resource.Layout.SelectCountryActivityCountryCell) { }
    }
}