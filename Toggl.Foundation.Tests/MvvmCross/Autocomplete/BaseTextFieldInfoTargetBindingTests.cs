using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Autocomplete;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.Autocomplete
{
    public sealed class BaseTextFieldInfoTargetBindingTests
    {
        public abstract class BaseTextFieldInfoTargetBindingTest : BaseMvvmCrossTests
        {
            protected TestableAutocompleteEventProvider EventProvider
                => TargetBinding.TestableEventProvider;

            protected TestableTextFieldInfoTargetBinding TargetBinding { get; }
                = new TestableTextFieldInfoTargetBinding(new object());

            protected BaseTextFieldInfoTargetBindingTest()
            {
                TargetBinding.SubscribeToEvents();
            }
        }

        public sealed class TheSubscribeToEventsMethod : BaseTextFieldInfoTargetBindingTest
        {
            [Fact, LogIfTooSlow]
            public void SubscribesToTheTagDeletedEvent()
            {
                EventProvider.TagDeletedEventCount.Should().Be(1);
            }

            [Fact, LogIfTooSlow]
            public void SubscribesToTheTextChangedEvent()
            {
                EventProvider.TextChangedEventCount.Should().Be(1);
            }

            [Fact, LogIfTooSlow]
            public void SubscribesToTheProjectDeletedEvent()
            {
                EventProvider.ProjectDeletedEventCount.Should().Be(1);
            }

            [Fact, LogIfTooSlow]
            public void SubscribesToTheCursorPositionChangedEvent()
            {
                EventProvider.CursorPositionChangedEventCount.Should().Be(1);
            }
        }

        public sealed class TheDisposeMethod : BaseTextFieldInfoTargetBindingTest
        {
            public TheDisposeMethod()
            {
                TargetBinding.Dispose();
            }

            [Fact, LogIfTooSlow]
            public void UnsubscribesToTheTagDeletedEvent()
            {
                EventProvider.TagDeletedEventCount.Should().Be(0);
            }

            [Fact, LogIfTooSlow]
            public void UnsubscribesToTheTextChangedEvent()
            {
                EventProvider.TextChangedEventCount.Should().Be(0);
            }

            [Fact, LogIfTooSlow]
            public void UnsubscribesToTheProjectDeletedEvent()
            {
                EventProvider.ProjectDeletedEventCount.Should().Be(0);
            }

            [Fact, LogIfTooSlow]
            public void UnsubscribesToTheCursorPositionChangedEvent()
            {
                EventProvider.CursorPositionChangedEventCount.Should().Be(0);
            }
        }

        public sealed class TheOnProjectDeletedMethod : BaseTextFieldInfoTargetBindingTest
        {
            [Fact, LogIfTooSlow]
            public void RemovesTheProjectInfoFromTheCurrentTextFieldInfo()
            {
                TargetBinding.SetValue(
                    TextFieldInfo
                        .Empty(1)
                        .WithTextAndCursor("Something", 1)
                        .WithProjectAndTaskInfo(1, 2, "some project", "#1e1e1e", 0, ""));

                EventProvider.RaiseProjectDeleted();

                TargetBinding.TextFieldInfo.ProjectId.Should().BeNull();
            }

            [Fact, LogIfTooSlow]
            public void SetsTheCursorPositionToTheEndOfTheDescription()
            {
                TargetBinding.SetValue(
                    TextFieldInfo
                        .Empty(1)
                        .WithTextAndCursor("Something", 1)
                        .WithProjectAndTaskInfo(1, 2, "some project", "#1e1e1e", 0, ""));

                EventProvider.RaiseProjectDeleted();

                TargetBinding.TextFieldInfo.CursorPosition.Should().Be("Something".Length);
            }
        }

        public sealed class TheOnTagDeletedMethod : BaseTextFieldInfoTargetBindingTest
        {
            [Theory, LogIfTooSlow]
            [InlineData(0)]
            [InlineData(1)]
            public void RemovesTheReportedTagFromTheCurrentTextFieldInfo(int index)
            {
                var workspace = Substitute.For<IThreadSafeWorkspace>();
                workspace.Name.Returns("Some workspace");
                var tag = Substitute.For<IThreadSafeTag>();
                tag.Name.Returns("Some tag");
                tag.Id.Returns(1);
                tag.Workspace.Returns(workspace);
                tag.WorkspaceId.Returns(3);
                var tag2 = Substitute.For<IThreadSafeTag>();
                tag2.Name.Returns("Some tag 2");
                tag2.Id.Returns(2);
                tag2.Workspace.Returns(workspace);
                tag2.WorkspaceId.Returns(3);
                TargetBinding.SetValue(
                    TextFieldInfo
                        .Empty(1)
                        .AddTag(new TagSuggestion(tag))
                        .AddTag(new TagSuggestion(tag2)));

                EventProvider.RaiseTagDeleted(index);

                TargetBinding.TextFieldInfo.Tags.Single().TagId.Should().Be(index == 0 ? 2 : 1);
            }
        }

        public sealed class TheQueueValueChangeMethod : BaseTextFieldInfoTargetBindingTest
        {
            [Fact, LogIfTooSlow]
            public void SetsTheCursorToTheEndOfTheDescriptionIfTheTextChangedInTokenRegion()
            {
                const string newDescription = "A ";
                TargetBinding.SetValue(
                    TextFieldInfo
                        .Empty(1)
                        .WithTextAndCursor("A", 0)
                        .WithProjectAndTaskInfo(1, 2, "some project", "#1e1e1e", 0, ""));
                TargetBinding.IsSelectingText = false;
                TargetBinding.CurrentCursorPosition = 4;
                TargetBinding.CurrentTimeEntryDescription = newDescription;

                EventProvider.RaiseTextChanged();

                TargetBinding.TextFieldInfo.CursorPosition.Should().Be(newDescription.Length);
            }
        }

        public sealed class TestableAutocompleteEventProvider : IAutocompleteEventProvider
        {
            public int TagDeletedEventCount
                => TagDeleted?.GetInvocationList().Length ?? 0;

            public int TextChangedEventCount
                => TextChanged?.GetInvocationList().Length ?? 0;

            public int ProjectDeletedEventCount
                => ProjectDeleted?.GetInvocationList().Length ?? 0;

            public int CursorPositionChangedEventCount
                => CursorPositionChanged?.GetInvocationList().Length ?? 0;

            public event EventHandler TextChanged;

            public event EventHandler ProjectDeleted;

            public event EventHandler CursorPositionChanged;

            public event EventHandler<TagDeletedEventArgs> TagDeleted;

            public void RaiseTextChanged()
            {
                TextChanged?.Invoke(this, EventArgs.Empty);
            }

            public void RaiseProjectDeleted()
            {
                ProjectDeleted?.Invoke(this, EventArgs.Empty);
            }

            public void RaiseCursorPositionChanged()
            {
                CursorPositionChanged?.Invoke(this, EventArgs.Empty);
            }

            public void RaiseTagDeleted(int tagIndex)
            {
                TagDeleted?.Invoke(this, new TagDeletedEventArgs(0, tagIndex));
            }
        }

        public sealed class TestableTextFieldInfoTargetBinding : BaseTextFieldInfoTargetBinding<object>
        {
            public TestableTextFieldInfoTargetBinding(object target)
                : base(target)
            {
            }

            protected override IAutocompleteEventProvider EventProvider => TestableEventProvider;

            public new TextFieldInfo TextFieldInfo => base.TextFieldInfo;

            public TestableAutocompleteEventProvider TestableEventProvider { get; }
                = new TestableAutocompleteEventProvider();

            public bool IsSelectingText { get; set; }

            public int CurrentCursorPosition { get; set; }

            public string CurrentTimeEntryDescription { get; set; }

            protected override void MarkViewForRedrawing() { }

            protected override void UpdateTarget(TextFieldInfo textFieldInfo) { }

            protected override bool CheckIfSelectingText() => IsSelectingText;

            protected override int GetCurrentCursorPosition() => CurrentCursorPosition;

            protected override string GetCurrentTimeEntryDescription() => CurrentTimeEntryDescription;
        }
    }
}
