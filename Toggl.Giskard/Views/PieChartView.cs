using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using MvvmCross.Platform.UI;
using MvvmCross.Plugins.Color.Droid;
using Toggl.Foundation.Reports;
using Toggl.Giskard.Extensions;
using System.Collections.Generic;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.PieChartView")]
    public sealed class PieChartView : View
    {
        private int padding;
        private const float fullCircle = 360.0f;
        private const double radToDegree = 180 / Math.PI;
        private readonly TextPaint textPaint = new TextPaint();

        private IEnumerable<ChartSegment> segments = new ChartSegment[0];
        public IEnumerable<ChartSegment> Segments
        {
            get => segments;
            set
            {
                segments = value;
                PostInvalidate();
            }
        }

        public PieChartView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public PieChartView(Context context)
            : base(context)
        {
            initialize(context);
        }

        public PieChartView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            initialize(context);
        }

        private void initialize(Context context)
        {
            padding = 16.DpToPixels(context);

            textPaint.Color = Color.White;
            textPaint.TextAlign = Paint.Align.Left;
            textPaint.TextSize = 10.SpToPixels(context);
            textPaint.SetTypeface(Typeface.Create("sans-serif", TypefaceStyle.Bold));
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, widthMeasureSpec);
        }

        public override void Draw(Canvas canvas)
        {
            var viewCenterX = Width * 0.5f;
            var viewCenterY = Height * 0.5f;
            var radius = viewCenterX;
            var totalSeconds = (float)Segments.Select(x => x.TrackedTime.TotalSeconds).Sum();

            var startDegrees = 270.0f;

            var oval = new RectF(0, 0, Width, Height);

            foreach (var segment in Segments)
            {
                var segmentPaint = new Paint();
                segmentPaint.Color = MvxColor.ParseHexString(segment.Color).ToAndroidColor();

                var percent = (float)segment.TrackedTime.TotalSeconds / totalSeconds;
                var sweepDegrees = fullCircle * percent;
                var endDegrees = (startDegrees + sweepDegrees) % fullCircle;

                // Draw arc
                canvas.DrawArc(oval, startDegrees, sweepDegrees, true, segmentPaint);

                // Disable drawing on segments that are too small
                if (ProjectSummaryReport.ShouldDraw(percent))
                {
                    // Save state for restoring later.
                    canvas.Save();

                    // Translate to draw the text
                    canvas.Translate(viewCenterX, viewCenterY);
                    canvas.Rotate(endDegrees + 180.0f);

                    // Draw the text
                    var integerPercentage = (int)(percent * 100);
                    var nameToDraw = segment.FormattedName();
                    var percentageToDraw = $"{integerPercentage}%";

                    var bounds = new Rect();
                    textPaint.GetTextBounds(nameToDraw, 0, nameToDraw.Length, bounds);
                    var textHeight = bounds.Height();

                    canvas.DrawText(nameToDraw, -radius + padding, padding, textPaint);
                    canvas.DrawText(percentageToDraw, -radius + padding, textHeight + padding * 1.5f, textPaint);

                    // Restore the original coordinate system.
                    canvas.Restore();
                }

                startDegrees = endDegrees;
            }
        }
    }
}
