using Android.Widget;
using MvvmCross.Binding;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.MvvmCross.Autocomplete;
using Toggl.Giskard.Autocomplete;
using Toggl.Giskard.Extensions;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Bindings
{
    public sealed class EditTextTextFieldInfoTargetBinding : BaseTextFieldInfoTargetBinding<EditText>
    {
        public const string BindingName = "TextFieldInfo";

        private readonly AutocompleteTextWatcher spanWatcher = new AutocompleteTextWatcher();

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override IAutocompleteEventProvider EventProvider => spanWatcher;

        private bool shouldUseLastKnownPosition;

        private int lastKnownPosition;

        public EditTextTextFieldInfoTargetBinding(EditText target)
            : base(target)
        {
            Target.AddTextChangedListener(spanWatcher);
        }

        protected override string GetCurrentTimeEntryDescription()
            => Target.GetDescription();

        protected override int GetCurrentCursorPosition()
            => Target.SelectionStart == 0 && shouldUseLastKnownPosition
                ? lastKnownPosition
                : Target.SelectionStart;

        protected override bool CheckIfSelectingText()
            => Target.SelectionStart != Target.SelectionEnd;

        protected override void UpdateTarget(TextFieldInfo textFieldInfo)
        {
            if (!Target.IsFocused) return;

            var formattedText = textFieldInfo.GetSpannableText();
            if (formattedText.ToString() == Target.TextFormatted?.ToString())
                return;

            lastKnownPosition = Target.SelectionStart;
            shouldUseLastKnownPosition = isProbablyEditingTokenRegion(textFieldInfo.Text);
            Target.TextFormatted = formattedText;
            Target.SetSelection(textFieldInfo.CursorPosition);
        }

        protected override void MarkViewForRedrawing()
        {
            Target.Invalidate();
        }

        private bool isProbablyEditingTokenRegion(string newDescription)
            => isProbablyEditingProjectRegion(newDescription)
               || isProbablyEditingTagsRegion(newDescription);

        private bool isProbablyEditingProjectRegion(string newDescription)
            => isNewDescriptionAddingToken(newDescription, QuerySymbols.ProjectsString)
               || isNumberOfProjectTokensSmallerThanBefore(newDescription);

        private bool isNumberOfProjectTokensSmallerThanBefore(string newDescription)
            => newDescription.CountOccurrences(QuerySymbols.Projects) <
               (Target.Text?.CountOccurrences(QuerySymbols.Projects) ?? 0);

        private bool isProbablyEditingTagsRegion(string newDescription)
            => isNewDescriptionAddingToken(newDescription, QuerySymbols.TagsString)
               || isNumberOfTagTokensDifferentThanBefore(newDescription);

        private bool isNumberOfTagTokensDifferentThanBefore(string newDescription)
            => newDescription.CountOccurrences(QuerySymbols.Tags) <
               (Target.Text?.CountOccurrences(QuerySymbols.Tags) ?? 0);

        private bool isNewDescriptionAddingToken(string newDescription, string token)
            => $"{Target.Text?.Trim() ?? string.Empty} {token}" == newDescription.Trim();
    }
}
