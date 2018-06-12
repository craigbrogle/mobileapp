using CoreGraphics;
using Foundation;
using UIKit;
using CoreAnimation;

namespace Toggl.Daneel.Extensions
{
    public static class ViewExtensions
    {
        private const int switchHeight = 24;

        public static void Resize(this UISwitch self)
        {
            var scale = switchHeight / self.Frame.Height;
            self.Transform = CGAffineTransform.MakeScale(scale, scale);
        }

        public static void Shake(this UIView view, double duration = 0.05, int repeatCount = 5, double shakeThreshold = 4.0)
        {
            var animation = CABasicAnimation.FromKeyPath("position");
            animation.Duration = duration;
            animation.RepeatCount = repeatCount;
            animation.AutoReverses = true;
            animation.From =
                NSValue.FromCGPoint(new CGPoint(view.Center.X - shakeThreshold, view.Center.Y));

            animation.To = NSValue.FromCGPoint(new CGPoint(view.Center.X + shakeThreshold, view.Center.Y));
            view.Layer.AddAnimation(animation, "shake");
        }
    }
}
