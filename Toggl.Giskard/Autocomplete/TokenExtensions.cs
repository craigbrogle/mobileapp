using System;
using System.Linq;
using System.Security.Cryptography;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using Java.Lang;
using Toggl.Foundation.Autocomplete;
using Toggl.Giskard.Autocomplete;
using Object = Java.Lang.Object;
using StringBuilder = System.Text.StringBuilder;

namespace Toggl.Giskard.Extensions
{
    public static class TokenExtensions
    {
        private const float spanSizeProportion = 0.8f;

        public static string GetDescription(this EditText self)
        {
            var spannable = self.TextFormatted as SpannableStringBuilder;
            if (spannable == null)
                return self.Text;

            var tokenSpans = spannable.GetSpans(0, spannable.Length(), Class.FromType(typeof(TokenSpan)));
            if (tokenSpans.Length == 0)
                return spannable.ToString();

            var descriptionBeforeTokenSpans = new StringBuilder(spannable.SubSequence(0, spannable.GetSpanStart(tokenSpans.First())));

            var descriptionUntilLastTokenSpan = tokenSpans.AsEnumerable()
                .SkipLast(1)
                .Select((span, spanIndex) => (leftSpanEnd: spannable.GetSpanEnd(span), rightSpanStart: spannable.GetSpanStart(tokenSpans[spanIndex + 1])))
                .Aggregate(descriptionBeforeTokenSpans,
                    (builder, spansBoundaries) =>
                        builder.Append(spannable.SubSequence(spansBoundaries.leftSpanEnd, spansBoundaries.rightSpanStart)));

            var fullDescription = descriptionUntilLastTokenSpan
                .Append(spannable.SubSequence(spannable.GetSpanEnd(tokenSpans.Last()), spannable.Length()));

            return fullDescription.ToString();
        }


        public static ISpannable GetSpannableText(this TextFieldInfo self)
        {
            var builder = new SpannableStringBuilder();

            builder.Append(self.Text);
            builder.AppendProjectToken(self);
            builder.AppendTagTokens(self);

            return builder;
        }

        public static void AppendProjectToken(this SpannableStringBuilder self, TextFieldInfo textFieldInfo)
        {
            if (textFieldInfo.ProjectId == null) return;

            var start = self.Length();
            self.Append(textFieldInfo.ProjectName);
            var end = self.Length();

            var projectColor = Color.ParseColor(textFieldInfo.ProjectColor);
            self.SetSpan(new ProjectTokenSpan(projectColor), start, end, SpanTypes.ExclusiveExclusive);
            self.SetSpan(new RelativeSizeSpan(spanSizeProportion), start, end, SpanTypes.ExclusiveExclusive);
        }

        public static void AppendTagTokens(this SpannableStringBuilder self, TextFieldInfo textFieldInfo)
        {
            for (int i = 0; i < textFieldInfo.Tags.Length; i++)
            {
                var tag = textFieldInfo.Tags[i];
                var start = self.Length();
                self.Append(tag.Name);
                var end = self.Length();

                self.SetSpan(new RelativeSizeSpan(spanSizeProportion), start, end, SpanTypes.ExclusiveExclusive);
                self.SetSpan(new TagsTokenSpan(i), start, end, SpanTypes.ExclusiveExclusive);
            }
        }
    }
}
