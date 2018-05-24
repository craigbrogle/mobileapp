using Toggl.Foundation.MvvmCross.Combiners;
using MvvmCross.Binding.Droid;
using System;

namespace Toggl.Giskard
{
    public sealed class TogglBindingBuilder : MvxAndroidBindingBuilder
    {
        protected override void FillValueCombiners(MvvmCross.Binding.Combiners.IMvxValueCombinerRegistry registry)
        {
            registry.AddOrOverwrite("Duration", new DurationValueCombiner());
            registry.AddOrOverwrite("DateTimeOffsetShortDateFormat", new DateTimeOffsetDateFormatValueCombiner(TimeZoneInfo.Local, false));
            registry.AddOrOverwrite("DateTimeOffsetTimeFormat", new DateTimeOffsetTimeFormatValueCombiner(TimeZoneInfo.Local));
            base.FillValueCombiners(registry);
        }
    }
}