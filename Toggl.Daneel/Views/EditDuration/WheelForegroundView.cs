using System;
using CoreAnimation;
using CoreGraphics;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Foundation.MvvmCross.Helper;
using Foundation;
using UIKit;
using MvvmCross.Platform.Core;
using static Toggl.Multivac.Math;
using Toggl.Daneel.Extensions;
using Toggl.Multivac.Extensions;

namespace Toggl.Daneel.Views.EditDuration
{
    [Register(nameof(WheelForegroundView))]
    public sealed class WheelForegroundView : BaseWheelView
    {
        private readonly CGColor backgroundColor = Color.EditDuration.Wheel.Duration.ToNativeColor().CGColor;

        private readonly CGColor capColor = Color.EditDuration.Wheel.Cap.ToNativeColor().CGColor;

        private readonly CGColor shadowColor = Color.EditDuration.Wheel.Shadow.ToNativeColor().CGColor;

        // The sizes are relative to the radius of the wheel.
        // The radius of the wheel in the design document is 128 points.
        private readonly nfloat capRadius = 14.05f / 128f;

        private readonly nfloat horizontalBarLength = 12f / 128f;

        private readonly nfloat horizontalBarHeight = 1.5f / 128f;

        private readonly nfloat horizontalBarsDistance = 3.1f / 128f;

        private readonly nfloat horizontalBarCornerRadius = 0.2f / 128f;

        private readonly nfloat shadowRadius = 4f / 128f;

        private readonly float shadowOpacity = 0.3f;

        private readonly nfloat triangleWidth = 9f / 128f;

        private readonly nfloat triangleHeight = 10f / 128f;

        private readonly nfloat triangleCenterHorizontalOffset = 1.4f / 128f;

        private readonly nfloat squareHeight = 10f / 128f;

        private readonly nfloat squareWidth = 10f / 128f;

        private readonly nfloat suqareCenterHorizontalOffset = 0f;

        private readonly nfloat extendedRadiusMultiplier = 1.5f;

        private double endPointsRadius => SmallRadius + (Radius - SmallRadius) / 2;

        private DateTimeOffset startTime;

        private DateTimeOffset endTime;

        private bool isRunning;

        private CGPoint startTimePosition;

        private CGPoint endTimePosition;

        private UITouch currentTouch;

        private UIImage startHandleImage;

        private UIImage endHandleImage;

        private UISelectionFeedbackGenerator feedbackGenerator;

        private WheelUpdateType updateType;

        private int numberOfFullLoops => (int)((EndTime - StartTime).TotalMinutes / MinutesInAnHour);

        private bool isFullCircle => numberOfFullLoops >= 1;

        private double startTimeAngle => startTime.LocalDateTime.TimeOfDay.ToAngleOnTheDial().ToPositiveAngle();

        private double endTimeAngle => endTime.LocalDateTime.TimeOfDay.ToAngleOnTheDial().ToPositiveAngle();

        private double editBothAtOnceStartTimeAngleOffset;

        public event EventHandler StartTimeChanged;

        public event EventHandler EndTimeChanged;

        public DateTimeOffset MinimumStartTime { get; set; }

        public DateTimeOffset MaximumStartTime { get; set; }

        public DateTimeOffset MinimumEndTime { get; set; }

        public DateTimeOffset MaximumEndTime { get; set; }

        public DateTimeOffset StartTime
        {
            get => startTime;
            set
            {
                if (startTime == value) return;
                startTime = value.Clamp(MinimumStartTime, MaximumStartTime);
                StartTimeChanged.Raise(this);
                SetNeedsLayout();
            }
        }

        public DateTimeOffset EndTime
        {
            get => endTime;
            set
            {
                if (endTime == value) return;
                endTime = value.Clamp(MinimumEndTime, MaximumEndTime);
                EndTimeChanged.Raise(this);
                SetNeedsLayout();
            }
        }

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                if (isRunning == value) return;
                isRunning = value;
                SetNeedsLayout();
            }
        }

        public WheelForegroundView(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            startHandleImage = UIImage.FromBundle("icStartLabel");
            endHandleImage = UIImage.FromBundle("icEndLabel");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing == false) return;

            startHandleImage.Dispose();
            endHandleImage.Dispose();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            RemoveSublayers();
            calculateEndPointPositions();

            if (isFullCircle)
            {
                var fullWheel = CreateWheelLayer(backgroundColor);
                Layer.AddSublayer(fullWheel);
            }

            var backgroundLayer = createBackgroundLayer();
            Layer.AddSublayer(backgroundLayer);

            if (IsRunning == false)
            {
                var endCap = createCap(endTimePosition, endHandleImage, squareWidth, squareHeight, suqareCenterHorizontalOffset);
                Layer.AddSublayer(endCap);
            }

            var startCap = createCap(startTimePosition, startHandleImage, triangleWidth, triangleHeight, triangleCenterHorizontalOffset);
            Layer.AddSublayer(startCap);
        }

        private void calculateEndPointPositions()
        {
            var center = Center.ToMultivacPoint();

            startTimePosition = PointOnCircumference(center, startTimeAngle, endPointsRadius).ToCGPoint();
            endTimePosition = PointOnCircumference(center, endTimeAngle, endPointsRadius).ToCGPoint();
        }

        #region Touch interaction

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            var touch = findValidTouch(touches);
            if (touch != null)
            {
                currentTouch = touch;
                feedbackGenerator = new UISelectionFeedbackGenerator();
                feedbackGenerator.Prepare();
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            if (currentTouch == null) return;

            double previousAngle;
            switch (updateType)
            {
                case WheelUpdateType.EditStartTime:
                    previousAngle = startTimeAngle;
                    break;
                case WheelUpdateType.EditEndTime:
                    previousAngle = endTimeAngle;
                    break;
                default:
                    previousAngle = startTimeAngle + editBothAtOnceStartTimeAngleOffset;
                    break;
            }

            var currentTapPosition = currentTouch.LocationInView(this);
            var currentAngle = AngleBetween(currentTapPosition.ToMultivacPoint(), Center.ToMultivacPoint());

            var angleChange = currentAngle - previousAngle;
            while (angleChange < -Math.PI) angleChange += FullCircle;
            while (angleChange > Math.PI) angleChange -= FullCircle;

            var timeChange = angleChange.AngleToTime();

            updateEditedTime(timeChange);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            finishTouchEditing();
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            if (currentTouch == null || currentTouch.Phase == UITouchPhase.Ended)
            {
                finishTouchEditing();
            }
        }

        private UITouch findValidTouch(NSSet touches)
        {
            foreach (UITouch touch in touches)
            {
                var position = touch.LocationInView(this);
                var intention = determineTapIntention(position);
                if (intention.HasValue)
                {
                    updateType = intention.Value;
                    if (updateType == WheelUpdateType.EditBothAtOnce)
                    {
                        editBothAtOnceStartTimeAngleOffset =
                            AngleBetween(position.ToMultivacPoint(), Center.ToMultivacPoint()) - startTimeAngle;
                    }

                    return touch;
                }
            }

            return null;
        }

        private WheelUpdateType? determineTapIntention(CGPoint position)
        {
            if (touchesStartCap(position))
                return WheelUpdateType.EditStartTime;

            if (!IsRunning && touchesEndCap(position))
                return WheelUpdateType.EditEndTime;

            if (touchesStartCap(position, extendedRadius: true))
                return WheelUpdateType.EditStartTime;

            if (!IsRunning && touchesEndCap(position, extendedRadius: true))
                return WheelUpdateType.EditEndTime;

            if (!IsRunning && isOnTheWheelBetweenStartAndStop(position))
                return WheelUpdateType.EditBothAtOnce;

            return null;
        }

        private bool touchesStartCap(CGPoint position, bool extendedRadius = false)
            => isCloseEnough(position, startTimePosition, calculateCapRadius(extendedRadius));

        private bool touchesEndCap(CGPoint position, bool extendedRadius = false)
            => isCloseEnough(position, endTimePosition, calculateCapRadius(extendedRadius));

        private nfloat calculateCapRadius(bool extendedRadius)
            => (extendedRadius ? extendedRadiusMultiplier : 1) * (Thickness / 2);

        private static bool isCloseEnough(CGPoint tapPosition, CGPoint endPoint, nfloat radius)
            => DistanceSq(tapPosition.ToMultivacPoint(), endPoint.ToMultivacPoint()) <= radius * radius;

        private bool isOnTheWheelBetweenStartAndStop(CGPoint point)
        {
            var distanceFromCenterSq = DistanceSq(Center.ToMultivacPoint(), point.ToMultivacPoint());

            if (distanceFromCenterSq < SmallRadius * SmallRadius
                || distanceFromCenterSq > Radius * Radius)
            {
                return false;
            }

            var angle = AngleBetween(point.ToMultivacPoint(), Center.ToMultivacPoint());
            return isFullCircle || angle.IsBetween(startTimeAngle, endTimeAngle);
        }

        private void updateEditedTime(TimeSpan diff)
        {
            var giveFeedback = false;
            var duration = EndTime - StartTime;

            if (updateType == WheelUpdateType.EditStartTime
                || updateType == WheelUpdateType.EditBothAtOnce)
            {
                var nextStartTime = (StartTime + diff).RoundToClosestMinute();
                giveFeedback = nextStartTime != StartTime;
                StartTime = nextStartTime;
            }

            if (updateType == WheelUpdateType.EditEndTime)
            {
                var nextEndTime = (EndTime + diff).RoundToClosestMinute();
                giveFeedback = nextEndTime != EndTime;
                EndTime = nextEndTime;
            }

            if (updateType == WheelUpdateType.EditBothAtOnce)
            {
                EndTime = StartTime + duration;
            }

            if (giveFeedback)
            {
                feedbackGenerator.SelectionChanged();
                feedbackGenerator.Prepare();
            }
        }

        private void finishTouchEditing()
        {
            currentTouch = null;
            feedbackGenerator = null;
        }

        #endregion

        #region Shape layers factories

        private CALayer createBackgroundLayer()
        {
            var capArcRadius = Thickness / 2f;

            var startAngle = (nfloat)startTimeAngle;
            var endAngle = (nfloat)endTimeAngle;

            // these angles become obvious when you draw a diagram and mark all the angles
            var startCapStartAngle = startAngle + (nfloat)Math.PI;
            var startCapEndAngle = startAngle;
            var endCapStartAngle = endAngle;
            var endCapEndAngle = endAngle + (nfloat)Math.PI;

            var durationArc = new UIBezierPath();

            durationArc.AddArc(startTimePosition, capArcRadius, startCapStartAngle, startCapEndAngle, true); // start cap
            durationArc.AddArc(Center, Radius, startAngle, endAngle, true); // outer arc
            durationArc.AddArc(endTimePosition, capArcRadius, endCapStartAngle, endCapEndAngle, true); // end cap
            durationArc.AddArc(Center, SmallRadius, endAngle, startAngle, false); // inner arc

            var layer = new CAShapeLayer();
            layer.Path = durationArc.CGPath;
            layer.FillColor = backgroundColor;

            if (isFullCircle)
            {
                // cap shadows
                var shadowPath = new UIBezierPath();
                shadowPath.AddArc(startTimePosition, capArcRadius, 0f, (nfloat)FullCircle, false);
                shadowPath.AddArc(endTimePosition, capArcRadius, 0f, (nfloat)FullCircle, true);

                layer.ShadowPath = shadowPath.CGPath;
                layer.ShadowColor = shadowColor;
                layer.ShadowRadius = Resize(shadowRadius);
                layer.ShadowOpacity = shadowOpacity;
                layer.ShadowOffset = CGSize.Empty;

                var wheelLayer = CreateWheelLayer(backgroundColor);
                layer.Mask = wheelLayer;
            }

            return layer;
        }

        private CALayer createCap(CGPoint center, UIImage image, nfloat imageWidth, nfloat imageHeight, nfloat centerHorizontalOffset)
        {
            var innerRadius = Resize(capRadius);
            var outerRadius = (Radius - SmallRadius) / 2;
            var centerDistance = SmallRadius + outerRadius;

            var outerPath = new UIBezierPath();
            outerPath.AddArc(center, outerRadius, 0, (nfloat)FullCircle, false);

            var backgroundLayer = new CAShapeLayer();
            backgroundLayer.Path = outerPath.CGPath;
            backgroundLayer.FillColor = backgroundColor;

            var innerPath = new UIBezierPath();
            innerPath.AddArc(center, innerRadius, 0, (nfloat)FullCircle, false);

            var circleLayer = new CAShapeLayer();
            circleLayer.Path = innerPath.CGPath;
            circleLayer.FillColor = capColor;

            var height = Resize(imageHeight);
            var width = Resize(imageWidth);
            var offset = Resize(centerHorizontalOffset);
            var frame = new CGRect(center.X - width / 2f + offset, center.Y - height / 2f, width, height);
            var imageLayer = new CALayer();
            imageLayer.Contents = image.CGImage;
            imageLayer.Frame = frame;
            circleLayer.AddSublayer(imageLayer);

            backgroundLayer.AddSublayer(circleLayer);

            return backgroundLayer;
        }

        #endregion

        private enum WheelUpdateType
        {
            EditStartTime,
            EditEndTime,
            EditBothAtOnce
        }
    }
}
